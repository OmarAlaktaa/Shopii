using Shopii.Domain.Entities.Products;

namespace Shopii.Application.Features.Products.DTOs
{
    public static class ProductMappingExtensions
    {
        public static ProductDTO ToDTO(this Product product)
        {
            return new ProductDTO
            {
                ProductId = product.ProductId,
                Name = product.Name,
                BrandId = product.BrandId,
                CategoryId = product.CategoryId,
                CostPrice = product.CostPrice,
                SoldPrice = product.SoldPrice,
                Description = product.Description,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}
