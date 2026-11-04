using Shopii.Application.Interfaces.Persistence;
using Shopii.Domain.Entities.Users;

namespace Shopii.Tests.Fakes
{
    public sealed class FakeRoleRepository : IRoleRepository
    {
        private readonly Dictionary<string, Role> _roles = new(StringComparer.OrdinalIgnoreCase);

        public void Seed(Role role) => _roles[role.RoleTitle] = role;

        public Task<Role?> GetByTitleAsync(string title, CancellationToken cancellationToken) =>
            Task.FromResult(_roles.TryGetValue(title, out var role) ? role : null);
    }
}