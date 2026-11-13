using Microsoft.AspNetCore.Http;
using Shopii.API.Middleware;
using Xunit;

namespace Shopii.Tests
{
    public class CorrelationIdMiddlewareTests
    {
        [Fact]
        public async Task IncomingCorrelationId_IsEchoedAndUsed()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers[CorrelationIdMiddleware.HeaderName] =
                "test-correlation-123";

            await InvokeAsync(context);

            Assert.Equal("test-correlation-123", context.TraceIdentifier);
            Assert.Equal(
                "test-correlation-123",
                context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString());
            Assert.Equal(
                "test-correlation-123",
                context.Items[CorrelationIdMiddleware.ItemKey] as string);
            Assert.False(string.IsNullOrWhiteSpace(
                context.Items[CorrelationIdMiddleware.TraceIdItemKey] as string));
        }

        [Fact]
        public async Task IncomingRequestId_IsUsedWhenCorrelationIdIsMissing()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers[CorrelationIdMiddleware.RequestIdHeaderName] =
                "request-123";

            await InvokeAsync(context);

            Assert.Equal(
                "request-123",
                context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString());
        }

        [Fact]
        public async Task MissingHeader_GeneratesCorrelationId()
        {
            var context = new DefaultHttpContext();

            await InvokeAsync(context);

            var correlationId = context.Response.Headers[
                CorrelationIdMiddleware.HeaderName].ToString();

            Assert.False(string.IsNullOrWhiteSpace(correlationId));
        }

        [Fact]
        public async Task UnsafeHeaderValue_IsNotEchoed()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers[CorrelationIdMiddleware.HeaderName] = "bad value";

            await InvokeAsync(context);

            var correlationId = context.Response.Headers[
                CorrelationIdMiddleware.HeaderName].ToString();

            Assert.NotEqual("bad value", correlationId);
            Assert.False(string.IsNullOrWhiteSpace(correlationId));
        }

        private static async Task InvokeAsync(HttpContext context)
        {
            var middleware = new CorrelationIdMiddleware(
                ctx => ctx.Response.WriteAsync("ok"));

            await middleware.InvokeAsync(context);
        }
    }
}
