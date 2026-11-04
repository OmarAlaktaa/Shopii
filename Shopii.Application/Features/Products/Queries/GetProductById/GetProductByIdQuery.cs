using MediatR;
using Shopii.Application.Features.Products.DTOs;

namespace Shopii.Application.Features.Products.Queries.GetProductById;

/// <summary>
/// Retrieves a single product by its identifier.
/// Returns null when no product exists with the given id.
/// </summary>
public record GetProductByIdQuery(string ProductId)
    : IRequest<ProductDTO?>;
