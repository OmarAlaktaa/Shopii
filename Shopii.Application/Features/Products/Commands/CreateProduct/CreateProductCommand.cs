using MediatR;
using Shopii.Application.Features.Products.DTOs;

namespace Shopii.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommand : IRequest<ProductDTO>
    {
        public string Name { get; set; } = null!;
        public int BrandId { get; set; }
        public int CategoryId { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SoldPrice { get; set; }
        public string Description { get; set; } = null!;
    }
}
