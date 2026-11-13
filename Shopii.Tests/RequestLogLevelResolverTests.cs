using Microsoft.AspNetCore.Http;
using Serilog.Events;
using Shopii.API.Extensions;
using Shopii.API.Logging;
using Xunit;

namespace Shopii.Tests
{
    public class RequestLogLevelResolverTests
    {
        [Fact]
        public void HealthProbe_IsVerbose()
        {
            var context = CreateContext("/health", StatusCodes.Status200OK);

            Assert.Equal(
                LogEventLevel.Verbose,
                SerilogExtensions.ResolveRequestLogLevel(context, null));
        }

        [Fact]
        public void UnhealthyHealthProbe_IsError()
        {
            var context = CreateContext(
                "/health",
                StatusCodes.Status503ServiceUnavailable);

            Assert.Equal(
                LogEventLevel.Error,
                SerilogExtensions.ResolveRequestLogLevel(context, null));
        }

        [Fact]
        public void SuccessfulRequest_IsInformation()
        {
            var context = CreateContext("/api/orders", StatusCodes.Status200OK);

            Assert.Equal(
                LogEventLevel.Information,
                SerilogExtensions.ResolveRequestLogLevel(context, null));
        }

        [Fact]
        public void ClientError_IsWarning()
        {
            var context = CreateContext("/api/orders/1", StatusCodes.Status404NotFound);

            Assert.Equal(
                LogEventLevel.Warning,
                SerilogExtensions.ResolveRequestLogLevel(context, null));
        }

        [Fact]
        public void ServerError_IsError()
        {
            var context = CreateContext("/api/orders", StatusCodes.Status500InternalServerError);

            Assert.Equal(
                LogEventLevel.Error,
                SerilogExtensions.ResolveRequestLogLevel(context, null));
        }

        [Fact]
        public void Exception_IsErrorEvenForHealthProbe()
        {
            var context = CreateContext("/health", StatusCodes.Status500InternalServerError);

            Assert.Equal(
                LogEventLevel.Error,
                SerilogExtensions.ResolveRequestLogLevel(
                    context,
                    new InvalidOperationException("boom")));
        }

        [Fact]
        public void AlreadyLoggedServerError_IsInformation()
        {
            var context = CreateContext("/api/orders", StatusCodes.Status500InternalServerError);
            context.Items[HttpContextItemKeys.ExceptionLogged] = true;

            Assert.Equal(
                LogEventLevel.Information,
                SerilogExtensions.ResolveRequestLogLevel(context, null));
        }

        private static DefaultHttpContext CreateContext(
            string path,
            int statusCode)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = path;
            context.Response.StatusCode = statusCode;
            return context;
        }
    }
}
