using MediatR;
using Shopii.Application.Features.Carts.DTOs;

namespace Shopii.Application.Features.Carts.Commands.RemoveCartItem
{
    /// <summary>
    /// Removes a product from the current user's cart. Removing a product that
    /// is not in the cart fails with a NotFound error.
    /// </summary>
    public sealed record RemoveCartItemCommand(string ProductId) : IRequest<CartDTO>;
}
