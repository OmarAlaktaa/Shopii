using System.Diagnostics;
using Serilog.Context;

namespace Shopii.API.Middleware;

/// <summary>
/// Establishes a correlation id for every request, echoes it back to the
/// caller, and pushes it (plus the W3C trace id) into Serilog's LogContext so
/// every downstream log line inherits both values.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    public const string RequestIdHeaderName = "X-Request-Id";
    public const string ItemKey = "CorrelationId";
    public const string TraceIdItemKey = "TraceId";

    private const int MaxCorrelationIdLength = 128;

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);
        var traceId = Activity.Current?.TraceId.ToString() ?? correlationId;

        // Make ASP.NET Core's own diagnostics use the same id.
        context.TraceIdentifier = correlationId;
        context.Items[ItemKey] = correlationId;
        context.Items[TraceIdItemKey] = traceId;

        // Set it now for middleware/tests that inspect headers before the
        // response starts, and again in OnStarting so it survives
        // Response.Clear() in the exception-handling middleware.
        context.Response.Headers[HeaderName] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty(ItemKey, correlationId))
        using (LogContext.PushProperty(TraceIdItemKey, traceId))
        {
            await _next(context);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        var incoming = context.Request.Headers[HeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(incoming))
        {
            incoming = context.Request.Headers[RequestIdHeaderName].FirstOrDefault();
        }

        if (!string.IsNullOrWhiteSpace(incoming))
        {
            incoming = incoming.Trim();

            if (incoming.Length <= MaxCorrelationIdLength && IsSafe(incoming))
            {
                return incoming;
            }
        }

        // Prefer the W3C activity id (honors an incoming traceparent),
        // then ASP.NET's per-request identifier.
        return Activity.Current?.Id ?? context.TraceIdentifier;
    }

    private static bool IsSafe(string value)
    {
        foreach (var character in value)
        {
            if (!char.IsAsciiLetterOrDigit(character)
                && character != '-'
                && character != '_'
                && character != '.'
                && character != ':')
            {
                return false;
            }
        }

        return true;
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
