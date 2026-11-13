using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shopii.API.Logging;
using Shopii.Application.Common.Exceptions;

namespace Shopii.API.Extensions
{
    /// <summary>
    /// Translates domain/application exceptions into consistent HTTP responses
    /// instead of leaking a 500 for expected failures.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NotFoundException ex)
            {
                LogRejected(context, HttpStatusCode.NotFound, ex);
                await WriteAsync(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (ForbiddenException ex)
            {
                LogRejected(context, HttpStatusCode.Forbidden, ex);
                await WriteAsync(context, HttpStatusCode.Forbidden, ex.Message);
            }
            catch (UnauthorizedException ex)
            {
                LogRejected(context, HttpStatusCode.Unauthorized, ex);
                await WriteAsync(context, HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (ValidationException ex)
            {
                LogRejected(context, HttpStatusCode.BadRequest, ex);
                await WriteAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // Safety net for range checks that live below the validation
                // pipeline (domain/application code). A bad client value is a
                // 400, never a 500.
                LogRejected(context, HttpStatusCode.BadRequest, ex);
                await WriteAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (ArgumentException ex)
            {
                LogRejected(context, HttpStatusCode.BadRequest, ex);
                await WriteAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (DbUpdateException ex)
            {
                context.Items[HttpContextItemKeys.ExceptionLogged] = true;

                // Typically a foreign key violation: the record is still
                // referenced by other rows and cannot be removed or updated.
                _logger.LogRequestConflict(
                    context.Request.Method,
                    context.Request.Path.Value ?? string.Empty,
                    ex);

                await WriteAsync(
                    context,
                    HttpStatusCode.Conflict,
                    "The operation could not be completed because the record is referenced by other data.");
            }
            catch (Exception ex)
            {
                context.Items[HttpContextItemKeys.ExceptionLogged] = true;

                // Anything not handled above is an unexpected server-side
                // failure. Log the full details but never leak them to the
                // client.
                _logger.LogUnhandledException(
                    context.Request.Method,
                    context.Request.Path.Value ?? string.Empty,
                    ex);

                await WriteAsync(
                    context,
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Prepares HTTP request details and delegates the actual logging to the logging extensions.
        /// </summary>
        private void LogRejected(
            HttpContext context,
            HttpStatusCode statusCode,
            Exception exception)
        {
            _logger.LogRequestRejected(
                (int)statusCode,
                context.Request.Method,
                context.Request.Path.Value ?? string.Empty,
                exception);
        }

        /// <summary>
        /// Writes a JSON response with the specified status code and message.
        /// </summary>
        private static async Task WriteAsync(
            HttpContext context,
            HttpStatusCode statusCode,
            string message)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var payload = JsonSerializer.Serialize(new { Message = message });
            await context.Response.WriteAsync(payload);
        }
    }

    public static class ExceptionHandlingExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(
            this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
