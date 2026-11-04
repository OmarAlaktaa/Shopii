using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
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
                _logger.LogWarning(ex, "Resource not found: {Message}", ex.Message);
                await WriteAsync(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (ForbiddenException ex)
            {
                _logger.LogWarning(ex, "Forbidden: {Message}", ex.Message);
                await WriteAsync(context, HttpStatusCode.Forbidden, ex.Message);
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning(ex, "Unauthorized: {Message}", ex.Message);
                await WriteAsync(context, HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed: {Message}", ex.Message);
                await WriteAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // Safety net for range checks that live below the validation
                // pipeline (domain/application code). A bad client value is a
                // 400, never a 500.
                _logger.LogWarning(ex, "Argument out of range: {Message}", ex.Message);
                await WriteAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument: {Message}", ex.Message);
                await WriteAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (DbUpdateException ex)
            {
                // Typically a foreign key violation: the record is still
                // referenced by other rows and cannot be removed or updated.
                _logger.LogWarning(ex, "Database update conflict.");
                await WriteAsync(
                    context,
                    HttpStatusCode.Conflict,
                    "The operation could not be completed because the record is referenced by other data.");
            }
            catch (Exception ex)
            {
                // Anything not handled above is an unexpected server-side
                // failure. Log the full details but never leak them to the
                // client.
                _logger.LogError(
                    ex,
                    "Unhandled exception while processing {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                await WriteAsync(
                    context,
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred.");
            }
        }

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
