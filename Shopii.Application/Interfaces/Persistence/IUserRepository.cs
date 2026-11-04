using Shopii.Domain.Entities.Users;

namespace Shopii.Application.Interfaces.Persistence
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
        Task<bool> AnyActiveByEmailAsync(string email, CancellationToken cancellationToken);
        Task<User?> GetByIdAsync(string userId, CancellationToken cancellationToken);
        Task AddAsync(User user, CancellationToken cancellationToken);
        Task UpdateAsync(User user, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(string userId, CancellationToken cancellationToken);
        Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
