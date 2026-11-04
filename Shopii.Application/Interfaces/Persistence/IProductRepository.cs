using Shopii.Domain.Entities.Products;

namespace Shopii.Application.Interfaces.Persistence
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(string productId, CancellationToken cancellationToken);
        Task AddAsync(Product product, CancellationToken cancellationToken);
        Task UpdateAsync(Product product, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(string productId, CancellationToken cancellationToken);
        Task<(IReadOnlyList<Product> Products, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
