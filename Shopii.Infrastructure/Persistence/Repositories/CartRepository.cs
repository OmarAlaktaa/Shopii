using Microsoft.EntityFrameworkCore;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Domain.Entities.Carts;

namespace Shopii.Infrastructure.Persistence.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ShopiiDbContext _context;

        public CartRepository(ShopiiDbContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetByUserIdAsync(string userId, CancellationToken cancellationToken)
        {
            // Tracked: cart mutations (add item, remove item, clear) are saved
            // through UpdateAsync.
            return await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .SingleOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        }

        public async Task AddAsync(Cart cart, CancellationToken cancellationToken)
        {
            await _context.Carts.AddAsync(cart, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Cart cart, CancellationToken cancellationToken)
        {
            // Stamped on every change; stored as "UpdatingAt" in the database.
            cart.UpdatedAt = DateTime.UtcNow;

            _context.Carts.Update(cart);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
