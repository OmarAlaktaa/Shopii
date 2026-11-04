using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Shopii.API.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Read and validate the JWT signing key.
            var jwtKey = configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException(
                    "Jwt:Key is missing or empty. Configure a Base64-encoded signing key " +
                    "of at least 32 bytes (256 bits). " +
                    "Development: dotnet user-secrets set \"Jwt:Key\" " +
                    "\"<base64-32-bytes>\" --project Shopii.API. " +
                    "Production: set the Jwt__Key environment variable to the same " +
                    "Base64 value.");
            }

            byte[] keyBytes;

            try
            {
                keyBytes = Convert.FromBase64String(jwtKey);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException(
                    "Jwt:Key must be a valid Base64 string. " +
                    "Development: dotnet user-secrets set \"Jwt:Key\" " +
                    "\"<base64-32-bytes>\" --project Shopii.API. " +
                    "Production: set the Jwt__Key environment variable.", ex);
            }

            if (keyBytes.Length < 32)
            {
                throw new InvalidOperationException(
                    $"Jwt:Key must decode to at least 32 bytes (256 bits) for " +
                    $"HMAC-SHA256 signing; the configured value decodes to " +
                    $"{keyBytes.Length} bytes. Generate a new key and store it in " +
                    $"user secrets (Development) or the Jwt__Key environment " +
                    $"variable (Production).");
            }

            // Read issuer and audience.
            var issuer = configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT issuer is missing.");

            var audience = configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "JWT audience is missing.");

            // Register JWT bearer authentication.
            services
                .AddAuthentication(
                    JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = issuer,

                            ValidateAudience = true,
                            ValidAudience = audience,

                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey =
                                new SymmetricSecurityKey(keyBytes),

                            ValidateLifetime = true
                        };
                });

            // Register authorization services.
            services.AddAuthorization(options =>
            {
                // Admin-only endpoints (e.g. managing any user's orders).
                // Role titles come from the "Roles" lookup table.
                options.AddPolicy(
                    "Admin",
                    policy => policy.RequireRole("Admin"));
            });

            return services;
        }
    }
}
