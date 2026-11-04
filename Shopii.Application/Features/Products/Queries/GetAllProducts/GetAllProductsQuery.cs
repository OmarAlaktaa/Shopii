using MediatR;
using Shopii.Application.Common.Models;
using Shopii.Application.Features.Products.DTOs;

namespace Shopii.Application.Features.Products.Queries.GetAllProducts
{
    // Describes the request to retrieve a specific page of products.
    public sealed record GetAllProductsQuery(
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<PagedResult<ProductDTO>>, IPagedRequest;
}
