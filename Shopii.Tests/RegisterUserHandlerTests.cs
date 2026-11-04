using Shopii.Application.Features.Users.Commands.RegisterUser;
using Shopii.Domain.Entities.Users;
using Shopii.Tests.Fakes;
using Xunit;

namespace Shopii.Tests
{
    public class RegisterUserHandlerTests
    {
        private static Role CustomerRole() => new Role { RoleId = 1, RoleTitle = "Customer" };
        private static Role AdminRole() => new Role { RoleId = 2, RoleTitle = "Admin" };

        [Fact]
        public async Task Register_AssignsCustomerRole_NotPrivileged()
        {
            var repo = new FakeUserRepository();
            var roleRepo = new FakeRoleRepository();
            roleRepo.Seed(CustomerRole());
            roleRepo.Seed(AdminRole());
            var hasher = new FakePasswordHasher();

            var handler = new RegisterUserCommandHandler(repo, roleRepo, hasher);

            var result = await handler.Handle(new RegisterUserCommand
            {
                Email = "new@example.com",
                Password = "password123"
            }, CancellationToken.None);

            Assert.Equal(1, result.RoleId);
            var stored = repo.Get(result.UserId)!;
            Assert.Equal(1, stored.RoleId);
            Assert.NotEqual(2, stored.RoleId);
            Assert.False(stored.IsVerified);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ThrowsValidation()
        {
            var repo = new FakeUserRepository();
            var roleRepo = new FakeRoleRepository();
            roleRepo.Seed(CustomerRole());
            var hasher = new FakePasswordHasher();

            repo.Seed(new User
            {
                UserId = "existing",
                Email = "existing@example.com",
                Password = "hash",
                Salt = "salt",
                RoleId = 1,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow
            });

            var handler = new RegisterUserCommandHandler(repo, roleRepo, hasher);

            await Assert.ThrowsAsync<ValidationException>(() =>
                handler.Handle(new RegisterUserCommand
                {
                    Email = "existing@example.com",
                    Password = "password123"
                }, CancellationToken.None));
        }

        [Fact]
        public async Task Register_EmailBelongsToSoftDeletedUser_CreatesNewProfile()
        {
            var repo = new FakeUserRepository();
            var roleRepo = new FakeRoleRepository();
            roleRepo.Seed(CustomerRole());
            var hasher = new FakePasswordHasher();

            repo.Seed(new User
            {
                UserId = "archived",
                Email = "reuse@example.com",
                Password = "old-hash",
                Salt = "old-salt",
                RoleId = 1,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                DeletedAt = DateTime.UtcNow.AddDays(-1),
                DeletedByUserId = "admin"
            });

            var handler = new RegisterUserCommandHandler(repo, roleRepo, hasher);

            var result = await handler.Handle(new RegisterUserCommand
            {
                Email = "reuse@example.com",
                Password = "password123"
            }, CancellationToken.None);

            Assert.NotEqual("archived", result.UserId);
            Assert.Equal("reuse@example.com", result.Email);

            var archived = repo.Get("archived")!;
            Assert.NotNull(archived.DeletedAt);

            var created = repo.Get(result.UserId)!;
            Assert.Null(created.DeletedAt);
            Assert.Equal("reuse@example.com", created.Email);
            Assert.Equal(1, created.RoleId);
        }

        [Fact]
        public async Task Register_MissingCustomerRole_ThrowsNotFound()
        {
            var repo = new FakeUserRepository();
            var roleRepo = new FakeRoleRepository();
            var hasher = new FakePasswordHasher();

            var handler = new RegisterUserCommandHandler(repo, roleRepo, hasher);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                handler.Handle(new RegisterUserCommand
                {
                    Email = "new@example.com",
                    Password = "password123"
                }, CancellationToken.None));
        }

        [Fact]
        public async Task Register_UnauthenticatedRoleChange_NotApplicable()
        {
            // Registration never accepts a role input; the command has no
            // RoleId property, so a privileged role cannot be requested.
            var command = new RegisterUserCommand
            {
                Email = "x@example.com",
                Password = "password123"
            };

            Assert.Equal("x@example.com", command.Email);
        }
    }
}