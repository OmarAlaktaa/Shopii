using Shopii.Application.Features.Users.DTOs;
using Shopii.Application.Features.Users.Queries.GetOwnProfile;
using Shopii.Domain.Entities.Users;
using Shopii.Tests.Fakes;
using Xunit;

namespace Shopii.Tests
{
    public class GetOwnProfileHandlerTests
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
        public async Task GetOwnProfile_Authenticated_ReturnsCallerProfile()
        {
            var repo = new FakeUserRepository();
            var current = new FakeCurrentUserService
            {
                UserId = "cust-1",
                Role = "Customer",
                IsAdmin = false
            };
            repo.Seed(SeedUser("cust-1", "cust1@example.com"));
            repo.Seed(SeedUser("cust-2", "cust2@example.com"));

            var handler = new GetOwnProfileQueryHandler(repo, current);

            var result = await handler.Handle(
                new GetOwnProfileQuery(),
                CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("cust-1", result!.UserId);
            Assert.Equal("cust1@example.com", result.Email);
        }

        [Fact]
        public async Task GetOwnProfile_Unauthenticated_ThrowsUnauthorized()
        {
            var repo = new FakeUserRepository();
            var current = new FakeCurrentUserService { UserId = null };
            repo.Seed(SeedUser("cust-1", "cust1@example.com"));

            var handler = new GetOwnProfileQueryHandler(repo, current);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                handler.Handle(new GetOwnProfileQuery(), CancellationToken.None));
        }

        [Fact]
        public async Task GetOwnProfile_MissingUser_ReturnsNull()
        {
            var repo = new FakeUserRepository();
            var current = new FakeCurrentUserService
            {
                UserId = "missing",
                Role = "Customer",
                IsAdmin = false
            };

            var handler = new GetOwnProfileQueryHandler(repo, current);

            var result = await handler.Handle(
                new GetOwnProfileQuery(),
                CancellationToken.None);

            Assert.Null(result);
        }
    }
}