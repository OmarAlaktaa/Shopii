using Shopii.Application.Features.Users.Commands.UpdateOwnProfile;
using Shopii.Application.Features.Users.DTOs;
using Shopii.Domain.Entities.Users;
using Shopii.Tests.Fakes;
using Xunit;

namespace Shopii.Tests
{
    public class UpdateOwnProfileHandlerTests
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
        public async Task SelfUpdate_UpdatesEmailAndPassword_KeepsPrivilegedFields()
        {
            var repo = new FakeUserRepository();
            var current = new FakeCurrentUserService { UserId = "cust-1", Role = "Customer", IsAdmin = false };
            var hasher = new FakePasswordHasher();
            repo.Seed(SeedUser(repo, "cust-1", "old@example.com", 2, false));

            var handler = new UpdateOwnProfileCommandHandler(repo, hasher, current);

            var result = await handler.Handle(new UpdateOwnProfileCommand
            {
                Email = "new@example.com",
                Password = "newpassword123"
            }, CancellationToken.None);

            Assert.Equal("new@example.com", result.Email);
            var stored = repo.Get("cust-1")!;
            Assert.Equal("hashed-newpassword123", stored.Password);
            Assert.Equal(2, stored.RoleId);
            Assert.False(stored.IsVerified);
        }

        [Fact]
        public async Task SelfUpdate_IgnoresClientSuppliedId()
        {
            var repo = new FakeUserRepository();
            var current = new FakeCurrentUserService { UserId = "cust-1", Role = "Customer", IsAdmin = false };
            var hasher = new FakePasswordHasher();
            repo.Seed(SeedUser(repo, "cust-1", "old@example.com", 2, false));
            repo.Seed(SeedUser(repo, "cust-2", "other@example.com", 1, true));

            var handler = new UpdateOwnProfileCommandHandler(repo, hasher, current);

            await handler.Handle(new UpdateOwnProfileCommand
            {
                UserId = "cust-2",
                Email = "mine@example.com",
                Password = null
            }, CancellationToken.None);

            Assert.Equal("mine@example.com", repo.Get("cust-1")!.Email);
            Assert.Equal("other@example.com", repo.Get("cust-2")!.Email);
        }

        [Fact]
        public async Task SelfUpdate_Unauthenticated_ThrowsUnauthorized()
        {
            var repo = new FakeUserRepository();
            var current = new FakeCurrentUserService { UserId = null };
            var hasher = new FakePasswordHasher();

            var handler = new UpdateOwnProfileCommandHandler(repo, hasher, current);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                handler.Handle(new UpdateOwnProfileCommand
                {
                    Email = "x@example.com"
                }, CancellationToken.None));
        }
    }
}