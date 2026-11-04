using MediatR;
using Shopii.Application.Features.Carts.DTOs;

namespace Shopii.Application.Features.Carts.Queries.GetMyCart
{
    /// <summary>
    /// Retrieves the cart of the currently logged-in user.
    /// Users without a cart get an empty one back instead of a 404.
    /// </summary>
    public sealed record GetMyCartQuery : IRequest<CartDTO>;
}
