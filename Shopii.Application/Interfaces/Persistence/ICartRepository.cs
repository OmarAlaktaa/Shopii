using Shopii.Domain.Entities.Carts;

namespace Shopii.Application.Interfaces.Persistence
{
    public interface ICartRepository
    {
        /// <summary>
        /// Loads the user's cart including its items (with products).
        /// The entity is tracked so callers can mutate it and call UpdateAsync.
        /// Returns null when the user has no cart yet.
        /// </summary>
        Task<Cart?> GetByUserIdAsync(string userId, CancellationToken cancellationToken);

        Task AddAsync(Cart cart, CancellationToken cancellationToken);

        Task UpdateAsync(Cart cart, CancellationToken cancellationToken);
    }
}
