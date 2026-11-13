using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Shopii.API.Extensions;
using Shopii.API.Logging;
using Xunit;

namespace Shopii.Tests
{
    public class ExceptionHandlingMiddlewareTests
    {
        [Fact]
        public async Task ArgumentOutOfRangeException_Returns400()
        {
            var response = await InvokeAsync(
                new ArgumentOutOfRangeException(
                    "pageSize",
                    "Page size must be between 1 and 100."));

            Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
            Assert.Contains("Message", response.Body);
        }

        [Fact]
        public async Task ArgumentException_Returns400()
        {
            var response = await InvokeAsync(
                new ArgumentException("The provided value is invalid."));

            Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ValidationException_Returns400()
        {
            var response = await InvokeAsync(
                new ValidationException("Page number must be at least 1."));

            Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
            Assert.Contains("Page number must be at least 1.", response.Body);
        }

        [Fact]
        public async Task UnhandledException_Returns500WithGenericBody()
        {
            var response = await InvokeAsync(
                new Exception("sensitive internal detail"));

            Assert.Equal(StatusCodes.Status500InternalServerError, response.StatusCode);
            Assert.Contains("An unexpected error occurred.", response.Body);
            Assert.DoesNotContain("sensitive internal detail", response.Body);
            Assert.Equal("application/json", response.ContentType);
        }

        /// <summary>
        /// Verifies that unhandled exceptions are marked as already logged.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous test operation.
        /// </returns>
        [Fact] // This attribute ensures that the middleware marks unhandled exceptions as already logged.
        public async Task UnhandledException_MarksExceptionAsAlreadyLogged()
        {
            var middleware = new ExceptionHandlingMiddleware(
                _ => throw new Exception("boom"),
                NullLogger<ExceptionHandlingMiddleware>.Instance);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            Assert.True(context.Items[HttpContextItemKeys.ExceptionLogged] is true);
        }

        private static async Task<(int StatusCode, string Body, string? ContentType)> InvokeAsync(
            Exception exception)
        {
            var middleware = new ExceptionHandlingMiddleware(
                _ => throw exception,
                NullLogger<ExceptionHandlingMiddleware>.Instance);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            context.Response.Body.Position = 0;
            using var reader = new StreamReader(context.Response.Body);
            var body = await reader.ReadToEndAsync();

            return (context.Response.StatusCode, body, context.Response.ContentType);
        }
    }
}
