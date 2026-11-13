using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopii.Application.Interfaces.Authentication;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Application.Interfaces.Services;
using Shopii.Infrastructure.Auditing;
using Shopii.Infrastructure.Authentication;
using Shopii.Infrastructure.Persistence;
using Shopii.Infrastructure.Persistence.Interceptors;
using Shopii.Infrastructure.Persistence.Repositories;

// The DependencyInjection class provides an extension method to register
// the infrastructure services, including the database context and repositories,
// into the dependency injection container. It retrieves the connection string
// from the configuration and configures the DbContext to use SQLite.

namespace Shopii.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");

        // Registered as scoped so the interceptor can resolve the current user
        // for the active request.
        services.AddScoped<SoftDeleteInterceptor>();

        services.AddDbContext<ShopiiDbContext>((serviceProvider, options) =>
            options
                .UseSqlite(connectionString)
                .AddInterceptors(serviceProvider.GetRequiredService<SoftDeleteInterceptor>()));

        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}