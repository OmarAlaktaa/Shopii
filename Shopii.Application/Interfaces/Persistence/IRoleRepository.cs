using Shopii.Domain.Entities.Users;

namespace Shopii.Application.Interfaces.Persistence
{
    public interface IRoleRepository
    {
        Task<Role?> GetByTitleAsync(
            string title,
            CancellationToken cancellationToken = default);
    }
}