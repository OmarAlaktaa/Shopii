using System.Text.Json;
using System.Threading.RateLimiting;

namespace Shopii.API.Extensions;

public static class RateLimitingExtensions
{
    public const string AuthPolicy = "auth";

    /// <summary>
    /// Protects the authentication endpoints from brute-force attempts and CPU
    /// exhaustion caused by repeated password hashing. Each client IP gets its
    /// own fixed window, and excess requests are rejected immediately with a
    /// 429 instead of reaching the handlers. Limits are read from the
    /// "RateLimiting" configuration section.
    /// </summary>
    public static IServiceCollection AddShopiiRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var permitLimit = ReadInteger(
            configuration,
            "RateLimiting:PermitLimit",
            minimum: 1);

        var windowSeconds = ReadInteger(
            configuration,
            "RateLimiting:WindowSeconds",
            minimum: 1);

        var queueLimit = ReadInteger(
            configuration,
            "RateLimiting:QueueLimit",
            minimum: 0);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/json";

                // JsonSerializer (rather than WriteAsJsonAsync) keeps the
                // PascalCase "Message" property used by the exception handling
                // middleware.
                var payload = JsonSerializer.Serialize(
                    new { Message = "Too many requests. Please try again later." });

                await context.HttpContext.Response.WriteAsync(
                    payload,
                    cancellationToken);
            };

            options.AddPolicy(AuthPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromSeconds(windowSeconds),
                        QueueLimit = queueLimit
                    }));
        });

        return services;
    }

    private static int ReadInteger(
        IConfiguration configuration,
        string key,
        int minimum)
    {
        var raw = configuration[key];

        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new InvalidOperationException(
                $"{key} is missing or empty. Configure it in appsettings.json " +
                $"under the \"RateLimiting\" section, or set the corresponding " +
                $"environment variable (e.g. RateLimiting__PermitLimit).");
        }

        if (!int.TryParse(raw, out var value) || value < minimum)
        {
            throw new InvalidOperationException(
                $"{key} must be an integer greater than or equal to {minimum}; " +
                $"the configured value was \"{raw}\".");
        }

        return value;
    }
}
