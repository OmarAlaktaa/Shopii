using Microsoft.EntityFrameworkCore;
using Shopii.Application.Interfaces.Authentication;
using Shopii.Domain.Entities.Users;
using Shopii.Infrastructure.Persistence;

namespace Shopii.API.Extensions
{
    /// <summary>
    /// Development-only seeding for the authentication bypass.
    ///
    /// The bypass in <see cref="DevelopmentAuthenticationExtensions"/> injects a
    /// fixed user id into every request. Without a matching row, user-scoped
    /// endpoints such as GET /api/users/me return 404 even though the caller
    /// looks authenticated. This helper creates that row once, so a fresh
    /// development database behaves like one where the bypass id was pointed
    /// at an existing account.
    ///
    /// This must only be called in the Development environment (see Program.cs).
    /// </summary>
    public static class DevelopmentSeedExtensions
    {
        private const string DefaultRoleTitle = "Customer";
        private const string DefaultEmail = "dev@shopii.local";

        public static async Task SeedDevelopmentUserAsync(
            this IServiceProvider services,
            IConfiguration configuration)
        {
            var userId = configuration["DevAuth:UserId"];

            // The bypass already ignores ids that are not GUIDs; do the same
            // here instead of failing startup.
            if (!Guid.TryParse(userId, out _) || userId is null)
            {
                return;
            }

            using var scope = services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ShopiiDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(DevelopmentSeedExtensions));

            // A soft-deleted row still owns the primary key, so the default
            // query filter must be bypassed for this check.
            var alreadyExists = await db.Users
                .IgnoreQueryFilters()
                .AnyAsync(user => user.UserId == userId);

            if (alreadyExists)
            {
                return;
            }

            var roleTitle = configuration["DevAuth:Role"] ?? DefaultRoleTitle;
            var role = await db.Roles
                .FirstOrDefaultAsync(candidate => candidate.RoleTitle == roleTitle);

            if (role is null)
            {
                logger.LogWarning(
                    "DevAuth user was not seeded: role '{Role}' does not exist in User_Roles.",
                    roleTitle);
                return;
            }

            var email = (configuration["DevAuth:Email"] ?? DefaultEmail)
                .Trim()
                .ToLowerInvariant();

            // The email index only covers live rows, so a soft-deleted match
            // does not conflict and is deliberately ignored.
            var emailInUse = await db.Users
                .AnyAsync(user => user.Email == email);

            if (emailInUse)
            {
                logger.LogWarning(
                    "DevAuth user was not seeded: email '{Email}' is already used by another account.",
                    email);
                return;
            }

            // The bypass never verifies a password, so this account is created
            // with a random one and cannot be used to sign in.
            var passwordHash = hasher.HashPassword(
                Guid.NewGuid().ToString("N"),
                out var salt);

            db.Users.Add(new User
            {
                UserId = userId,
                Email = email,
                Password = passwordHash,
                Salt = salt,
                RoleId = role.RoleId,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();

            logger.LogInformation(
                "Seeded development user {UserId} ({Email}) with role '{Role}' for the DevAuth bypass.",
                userId,
                email,
                roleTitle);
        }
    }
}
