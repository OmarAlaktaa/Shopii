using MediatR;
using Shopii.Application.Features.Carts.DTOs;

namespace Shopii.Application.Features.Carts.Commands.UpdateCartItemQuantity
{
    /// <summary>
    /// Sets the quantity of a product that is already in the current user's
    /// cart (absolute value, not a delta), which makes the call idempotent.
    /// The cart is never created here: a product that is not in the cart fails
    /// with a NotFound error. Removing a line stays on RemoveCartItemCommand.
    /// </summary>
    public sealed record UpdateCartItemQuantityCommand(
        string ProductId,
        int Quantity) : IRequest<CartDTO>;
    }
