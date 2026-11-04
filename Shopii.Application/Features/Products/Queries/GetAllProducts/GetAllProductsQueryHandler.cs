using MediatR;
using Shopii.Application.Common.Models;
using Shopii.Application.Features.Products.DTOs;
using Shopii.Application.Interfaces.Persistence;

namespace Shopii.Application.Features.Products.Queries.GetAllProducts
{
    public sealed class GetAllProductsQueryHandler
        : IRequestHandler<GetAllProductsQuery, PagedResult<ProductDTO>>
    {
        private readonly IProductRepository _productRepository;

        public GetAllProductsQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<PagedResult<ProductDTO>> Handle(
            GetAllProductsQuery request,
            CancellationToken cancellationToken)
        {
            // Fetch only the requested page from the database.
            var (products, totalCount) =
                await _productRepository.GetPagedAsync(
                    request.PageNumber,
                    request.PageSize,
                    cancellationToken);

            // Convert domain entities into DTOs.
            var productDTOs = products
                .Select(product => product.ToDTO())
                .ToList();

            // Return the products together with pagination metadata.
            return new PagedResult<ProductDTO>(
                Items: productDTOs,
                PageNumber: request.PageNumber,
                PageSize: request.PageSize,
                TotalCount: totalCount);
        }
    }
}
