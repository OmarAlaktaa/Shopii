using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Shopii.API.Logging;
using Shopii.API.Middleware;

namespace Shopii.API.Extensions;

public static class SerilogExtensions
{
    /// <summary>
    /// Replaces the bootstrap logger with the fully configured Serilog pipeline.
    /// Levels, sinks, enrichers, and properties come from appsettings*.json.
    /// </summary>
    public static WebApplicationBuilder AddShopiiSerilog(
        this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            // Sink/configuration failures should be visible locally instead of
            // silently dropping events.
            SelfLog.Enable(Console.Error);
        }

        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Destructure.With<SensitiveDataDestructuringPolicy>());

        return builder;
    }

    /// <summary>
    /// Replaces the default ASP.NET Core request logging. Successful health
    /// probes are logged at Verbose so they disappear at Information and above;
    /// failing probes still surface at Warning/Error.
    /// </summary>
    public static IApplicationBuilder UseShopiiRequestLogging(
        this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            options.GetLevel = (httpContext, _, exception) =>
                ResolveRequestLogLevel(httpContext, exception);

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                // Path only: never log the query string, headers, or body.
                diagnosticContext.Set(
                    "RequestPath",
                    httpContext.Request.Path.Value ?? string.Empty);

                diagnosticContext.Set(
                    "RequestHost",
                    httpContext.Request.Host.Value ?? string.Empty);

                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);

                if (httpContext.Items.TryGetValue(
                        CorrelationIdMiddleware.ItemKey,
                        out var correlationId)
                    && correlationId is string correlationIdValue)
                {
                    diagnosticContext.Set("CorrelationId", correlationIdValue);
                }

                if (httpContext.Items.TryGetValue(
                        CorrelationIdMiddleware.TraceIdItemKey,
                        out var traceId)
                    && traceId is string traceIdValue)
                {
                    diagnosticContext.Set("TraceId", traceIdValue);
                }

                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    var userId = httpContext.User
                        .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                    if (!string.IsNullOrEmpty(userId))
                    {
                        diagnosticContext.Set("UserId", userId);
                    }
                }
            };
        });

        return app;
    }

    /// <summary>
    /// Chooses the level for the single summary event emitted per request.
    /// Detailed exception entries are written by the exception middleware; the
    /// summary is downgraded to Information when that already happened so one
    /// failure does not produce two Error entries.
    /// </summary>
    public static LogEventLevel ResolveRequestLogLevel(
        HttpContext httpContext,
        Exception? exception)
    {
        if (exception is not null)
        {
            return LogEventLevel.Error;
        }

        if (httpContext.Request.Path.StartsWithSegments("/health")
            && httpContext.Response.StatusCode < 400)
        {
            // Successful probes are noise; an unhealthy probe still falls
            // through to the Warning/Error rules below.
            return LogEventLevel.Verbose;
        }

        if (httpContext.Items.TryGetValue(
                HttpContextItemKeys.ExceptionLogged,
                out var exceptionLogged)
            && exceptionLogged is true)
        {
            return LogEventLevel.Information;
        }

        if (httpContext.Response.StatusCode >= 500)
        {
            return LogEventLevel.Error;
        }

        return httpContext.Response.StatusCode >= 400
            ? LogEventLevel.Warning
            : LogEventLevel.Information;
    }
}
