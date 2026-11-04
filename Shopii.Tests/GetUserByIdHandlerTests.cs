using Shopii.Application.Features.Users.DTOs;
using Shopii.Application.Features.Users.Queries.GetUserById;
using Shopii.Domain.Entities.Users;
using Shopii.Tests.Fakes;
using Xunit;

namespace Shopii.Tests
{
    public class GetUserByIdHandlerTests
    {
        private static User SeedUser(string id, string email) =>
            new User
            {
                UserId = id,
                Email = email,
                Password = "secret-hash",
                Salt = "secret-salt",
                RoleId = 2,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };

        [Fact]
        public async Task SelfAccess_ReturnsOwnProfile()
        {
            var repo = new FakeUserRepository();
            var current = new FakeCurrentUserService
            {
                UserId = "cust-1",
                Role = "Customer",
                IsAdmin = false
            };
            repo.Seed(SeedUser("cust-1", "cust1@example.com"));

            var handler = new GetUserByIdQueryHandler(repo, current);

            var result = await handler.Handle(
                new GetUserByIdQuery("cust-1"),
                CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("cust-1", result!.UserId);
        }

        [Fact]
        public async Task AdminAccessAnother_ReturnsTargetProfile()
        {
            var repo = new FakeUserRepository();
            var current = new FakeCurrentUserService
            {
                UserId = "admin-1",
                Role = "Admin",
                IsAdmin = true
            };
            repo.Seed(SeedUser("cust-2", "cust2@example.com"));

            var handler = new GetUserByIdQueryHandler(repo, current);

            var result = await handler.Handle(
                new GetUserByIdQuery("cust-2"),
                CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("cust-2", result!.UserId);
        }

        [Fact]
        public async Task CustomerAccessAnother_ReturnsNull()
        {
            var repo = new FakeUserRepository();
            var current = new FakeCurrentUserService
            {
                UserId = "cust-1",
                Role = "Customer",
                IsAdmin = false
            };
            repo.Seed(SeedUser("cust-2", "cust2@example.com"));

            var handler = new GetUserByIdQueryHandler(repo, current);

            var result = await handler.Handle(
                new GetUserByIdQuery("cust-2"),
                CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task NonexistentId_ReturnsNull()
        {
            var repo = new FakeUserRepository();
            var current = new FakeCurrentUserService
            {
                UserId = "admin-1",
                Role = "Admin",
                IsAdmin = true
            };

            var handler = new GetUserByIdQueryHandler(repo, current);

            var result = await handler.Handle(
                new GetUserByIdQuery("missing"),
                CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task Unauthenticated_ThrowsUnauthorized()
        {
            var repo = new FakeUserRepository();
            var current = new FakeCurrentUserService { UserId = null };
            repo.Seed(SeedUser("cust-2", "cust2@example.com"));

            var handler = new GetUserByIdQueryHandler(repo, current);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                handler.Handle(new GetUserByIdQuery("cust-2"), CancellationToken.None));
        }
    }
}