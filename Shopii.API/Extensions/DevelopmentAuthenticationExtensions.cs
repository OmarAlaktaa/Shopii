using System.Security.Claims;

namespace Shopii.API.Extensions
{
    /// <summary>
    /// Development-only authentication bypass.
    ///
    /// Injects a fake signed-in user into every request so that endpoints
    /// protected with [Authorize] can be tested without logging in and
    /// without dealing with short-lived JWT tokens.
    ///
    /// The fake identity is read from configuration ("DevAuth" section in
    /// appsettings.Development.json) and can be overridden per request
    /// with headers (X-Dev-User-Id, X-Dev-Role, X-Dev-Email).
    ///
    /// This middleware MUST only be registered in the Development
    /// environment (see Program.cs).
    /// </summary>
    public class DevelopmentAuthenticationMiddleware
    {
        private const string DefaultRole = "Admin";
        private const string DefaultEmail = "dev@shopii.local";

        // Optional per-request overrides, e.g. "X-Dev-Role: Customer".
        private const string UserIdHeader = "X-Dev-User-Id";
        private const string RoleHeader = "X-Dev-Role";
        private const string EmailHeader = "X-Dev-Email";

        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public DevelopmentAuthenticationMiddleware(
            RequestDelegate next,
            IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // If the request carries a real JWT, skip the bypass and let
            // the normal JwtBearer pipeline validate it. This keeps the
            // real authentication path testable while developing.
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                await _next(context);
                return;
            }

            var userId = ResolveUserId(context);

            if (userId is null)
            {
                // No usable identity configured: behave as if the bypass
                // were off, so [Authorize] still returns 401.
                await _next(context);
                return;
            }

            var role = ResolveHeader(context, RoleHeader)
                ?? _configuration["DevAuth:Role"]
                ?? DefaultRole;

            var email = ResolveHeader(context, EmailHeader)
                ?? _configuration["DevAuth:Email"]
                ?? DefaultEmail;

            // Same claims that TokenService.GenerateToken issues, so the
            // rest of the application behaves exactly as after a real login.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "DevAuth");
            context.User = new ClaimsPrincipal(identity);

            await _next(context);
        }

        private string? ResolveUserId(HttpContext context)
        {
            var userId = ResolveHeader(context, UserIdHeader)
                ?? _configuration["DevAuth:UserId"];

            // User ids are GUIDs in this database; ignore anything else
            // so a typo cannot produce a half-authenticated request.
            return Guid.TryParse(userId, out _) ? userId : null;
        }

        private static string? ResolveHeader(HttpContext context, string header)
        {
            var value = context.Request.Headers[header].FirstOrDefault();

            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }

    public static class DevelopmentAuthenticationExtensions
    {
        public static IApplicationBuilder UseDevelopmentAuthentication(
            this IApplicationBuilder app)
        {
            return app.UseMiddleware<DevelopmentAuthenticationMiddleware>();
        }
    }
}
