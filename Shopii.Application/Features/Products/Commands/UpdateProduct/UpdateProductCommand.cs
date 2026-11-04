using MediatR;
using Shopii.Application.Features.Products.DTOs;

namespace Shopii.Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductCommand : IRequest<ProductDTO>
    {
        public string ProductId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int BrandId { get; set; }
        public int CategoryId { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SoldPrice { get; set; }
        public string Description { get; set; } = null!;
    }
}
