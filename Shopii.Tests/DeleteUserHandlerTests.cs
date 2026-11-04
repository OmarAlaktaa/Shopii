using Shopii.Application.Features.Users.Commands.DeleteUser;
using Shopii.Domain.Entities.Users;
using Shopii.Tests.Fakes;
using Xunit;

namespace Shopii.Tests
{
    public class DeleteUserHandlerTests
    {
        private static User SeedUser(string id, string email) =>
            new User
            {
                UserId = id,
                Email = email,
                Password = "old-hash",
                Salt = "old-salt",
                RoleId = 2,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow
            };

        [Fact]
        public async Task SelfDelete_DeletesAccount()
        {
            var repo = new FakeUserRepository();
            var current = new FakeCurrentUserService
            {
                UserId = "cust-1",
                Role = "Customer",
                IsAdmin = false
            };
            repo.Seed(SeedUser("cust-1", "cust1@example.com"));

            var handler = new DeleteUserCommandHandler(repo, current);

            var result = await handler.Handle(
                new DeleteUserCommand("cust-1"),
                CancellationToken.None);

            Assert.True(result);
            Assert.Null(repo.Get("cust-1"));
        }

        [Fact]
        public async Task AdminDeleteAnotherUser_DeletesAccount()
        {
            var repo = new FakeUserRepository();
            var current = new FakeCurrentUserService
            {
                UserId = "admin-1",
                Role = "Admin",
                IsAdmin = true
            };
            repo.Seed(SeedUser("cust-2", "cust2@example.com"));

            var handler = new DeleteUserCommandHandler(repo, current);

            var result = await handler.Handle(
                new DeleteUserCommand("cust-2"),
                CancellationToken.None);

            Assert.True(result);
            Assert.Null(repo.Get("cust-2"));
        }

        [Fact]
        public async Task CustomerDeleteAnotherUser_ThrowsForbidden_AndDoesNotDelete()
        {
            var repo = new FakeUserRepository();
            var current = new FakeCurrentUserService
            {
                UserId = "cust-1",
                Role = "Customer",
                IsAdmin = false
            };
            repo.Seed(SeedUser("cust-2", "cust2@example.com"));

            var handler = new DeleteUserCommandHandler(repo, current);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                handler.Handle(new DeleteUserCommand("cust-2"), CancellationToken.None));

            Assert.NotNull(repo.Get("cust-2"));
        }

        [Fact]
        public async Task Unauthenticated_ThrowsUnauthorized()
        {
            var repo = new FakeUserRepository();
            var current = new FakeCurrentUserService { UserId = null };
            repo.Seed(SeedUser("cust-2", "cust2@example.com"));

            var handler = new DeleteUserCommandHandler(repo, current);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                handler.Handle(new DeleteUserCommand("cust-2"), CancellationToken.None));

            Assert.NotNull(repo.Get("cust-2"));
        }
    }
}