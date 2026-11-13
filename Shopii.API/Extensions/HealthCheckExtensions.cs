using Shopii.Infrastructure.Persistence;

namespace Shopii.API.Extensions;

/// <summary>
/// Provides extension methods for configuring Shopii health checks.
/// </summary>
/// <remarks>
/// Registers a health check for the database and maps an anonymous health check endpoint at
/// <c>/health</c>.
/// </remarks>
public static class HealthCheckExtensions
{
    public static IServiceCollection AddShopiiHealthChecks(
        this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<ShopiiDbContext>("database");

        return services;
    }

    public static IEndpointRouteBuilder MapShopiiHealthChecks(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health").AllowAnonymous();

        return endpoints;
    }
}
