using Microsoft.EntityFrameworkCore;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Domain.Entities.Products;

namespace Shopii.Infrastructure.Persistence.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ShopiiDbContext _context;

        public ProductRepository(ShopiiDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(string productId, CancellationToken cancellationToken)
        {
            return await _context.Products
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
        }

        public async Task AddAsync(Product product, CancellationToken cancellationToken)
        {
            await _context.Products.AddAsync(product, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Product product, CancellationToken cancellationToken)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> DeleteAsync(string productId, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .SingleOrDefaultAsync(p => p.ProductId == productId, cancellationToken);

            if (product is null)
            {
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<(IReadOnlyList<Product> Products, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            // Only reading data.
            var query = _context.Products.AsNoTracking();

            // Count all matching products before applying pagination.
            var totalCount = await query.CountAsync(cancellationToken);

            // Retrieve only the requested page.
            var products = await query
                .OrderByDescending(product => product.CreatedAt)
                .ThenBy(product => product.ProductId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (products, totalCount);
        }
    }
}
