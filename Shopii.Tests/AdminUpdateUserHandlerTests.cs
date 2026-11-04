using Shopii.Application.Features.Users.Commands.AdminUpdateUser;
using Shopii.Domain.Entities.Users;
using Shopii.Tests.Fakes;
using Xunit;

namespace Shopii.Tests
{
    public class AdminUpdateUserHandlerTests
    {
        private static User SeedUser(FakeUserRepository repo, string id, string email, int roleId, bool verified) =>
            new User
            {
                UserId = id,
                Email = email,
                Password = "old-hash",
                Salt = "old-salt",
                RoleId = roleId,
                IsVerified = verified,
                CreatedAt = DateTime.UtcNow
            };

        [Fact]
        public async Task AdminUpdate_UpdatesPrivilegedFields()
        {
            var repo = new FakeUserRepository();
            var current = new FakeCurrentUserService { UserId = "admin-1", Role = "Admin", IsAdmin = true };
            var hasher = new FakePasswordHasher();
            repo.Seed(SeedUser(repo, "cust-2", "old@example.com", 2, false));

            var handler = new AdminUpdateUserCommandHandler(repo, hasher, current);

            var result = await handler.Handle(new AdminUpdateUserCommand
            {
                UserId = "cust-2",
                Email = "updated@example.com",
                RoleId = 1,
                IsVerified = true,
                Password = "adminpass123"
            }, CancellationToken.None);

            Assert.Equal("updated@example.com", result.Email);
            Assert.Equal(1, result.RoleId);
            Assert.True(result.IsVerified);
            var stored = repo.Get("cust-2")!;
            Assert.Equal("hashed-adminpass123", stored.Password);
        }

        [Fact]
        public async Task AdminUpdate_NonAdmin_ThrowsUnauthorized()
        {
            var repo = new FakeUserRepository();
            var current = new FakeCurrentUserService { UserId = "cust-1", Role = "Customer", IsAdmin = false };
            var hasher = new FakePasswordHasher();
            repo.Seed(SeedUser(repo, "cust-2", "old@example.com", 2, false));

            var handler = new AdminUpdateUserCommandHandler(repo, hasher, current);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                handler.Handle(new AdminUpdateUserCommand
                {
                    UserId = "cust-2",
                    Email = "updated@example.com",
                    RoleId = 1,
                    IsVerified = true
                }, CancellationToken.None));
        }

        [Fact]
        public async Task AdminUpdate_Unauthenticated_ThrowsUnauthorized()
        {
            var repo = new FakeUserRepository();
            var current = new FakeCurrentUserService { UserId = null };
            var hasher = new FakePasswordHasher();

            var handler = new AdminUpdateUserCommandHandler(repo, hasher, current);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                handler.Handle(new AdminUpdateUserCommand
                {
                    UserId = "cust-2",
                    Email = "updated@example.com",
                    RoleId = 1,
                    IsVerified = true
                }, CancellationToken.None));
        }

        [Fact]
        public async Task AdminUpdate_MissingTarget_ThrowsNotFound()
        {
            var repo = new FakeUserRepository();
            var current = new FakeCurrentUserService { UserId = "admin-1", Role = "Admin", IsAdmin = true };
            var hasher = new FakePasswordHasher();

            var handler = new AdminUpdateUserCommandHandler(repo, hasher, current);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                handler.Handle(new AdminUpdateUserCommand
                {
                    UserId = "missing",
                    Email = "updated@example.com",
                    RoleId = 1,
                    IsVerified = true
                }, CancellationToken.None));
        }
    }
}