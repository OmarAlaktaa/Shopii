using Shopii.Application.Interfaces.Persistence;
using Shopii.Domain.Entities.Users;

namespace Shopii.Tests.Fakes
{
    public sealed class FakeUserRepository : IUserRepository
    {
        private readonly Dictionary<string, User> _users = new();

        public void Seed(User user) => _users[user.UserId] = user;

        public User? Get(string userId) =>
            _users.TryGetValue(userId, out var user) ? user : null;

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
            // Mirrors the EF global query filter: soft-deleted users are hidden.
            Task.FromResult(_users.Values.FirstOrDefault(u =>
                u.DeletedAt == null &&
                string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> AnyActiveByEmailAsync(string email, CancellationToken cancellationToken) =>
            Task.FromResult(_users.Values.Any(u =>
                u.DeletedAt == null &&
                string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)));

        public Task<User?> GetByIdAsync(string userId, CancellationToken cancellationToken) =>
            Task.FromResult(_users.TryGetValue(userId, out var user) ? user : null);

        public Task AddAsync(User user, CancellationToken cancellationToken)
        {
            _users[user.UserId] = user;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(User user, CancellationToken cancellationToken)
        {
            _users[user.UserId] = user;
            return Task.CompletedTask;
        }

        public Task<bool> DeleteAsync(string userId, CancellationToken cancellationToken) =>
            Task.FromResult(_users.Remove(userId));

        public Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, CancellationToken cancellationToken = default) =>
            Task.FromResult(((IReadOnlyList<User>)_users.Values.ToList(), _users.Count));
    }
}