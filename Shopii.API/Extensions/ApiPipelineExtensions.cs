using Shopii.API.Middleware;

namespace Shopii.API.Extensions;

/// <summary>
/// Provides extension methods for configuring the Shopii API HTTP request pipeline.
/// </summary>
public static class ApiPipelineExtensions
{
    /// <summary>
    /// Configures the Shopii HTTP request pipeline.
    /// Registers middleware in the required order to handle request
    /// correlation, logging, exception handling, development authentication,
    /// HTTPS redirection, routing, rate limiting, authentication,
    /// and authorization before executing API endpoints.
    /// Ensures health checks remain accessible over HTTP inside containers.
    /// </summary>
    public static WebApplication UseShopiiApiPipeline(this WebApplication app)
    {
        // Correlation first: every log written by the pipeline below inherits
        // CorrelationId/TraceId, and the response carries X-Correlation-Id.
        app.UseCorrelationId();

        // Replaces the default ASP.NET Core request logging.
        app.UseShopiiRequestLogging();

        app.UseExceptionHandling();

        if (app.Environment.IsDevelopment())
        {
            // Keep the dev bypass inside the correlated/exception-handled
            // pipeline.
            app.UseDevelopmentAuthentication();
        }

        // Health probes must be reachable over plain HTTP inside containers,
        // so they bypass HTTPS redirection.
        app.UseWhen(
            context => !context.Request.Path.StartsWithSegments("/health"),
            branch => branch.UseHttpsRedirection());

        // Routing must run before the rate limiter so endpoint metadata such as
        // [EnableRateLimiting("auth")] is available when the request is limited.
        app.UseRouting();
        app.UseRateLimiter();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapShopiiHealthChecks();

        return app;
    }
}
