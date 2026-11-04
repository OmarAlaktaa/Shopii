using MediatR;
using Shopii.Application.Features.Carts.DTOs;

namespace Shopii.Application.Features.Carts.Commands.AddCartItem
{
    /// <summary>
    /// Adds a product to the current user's cart (creating the cart on first
    /// use). When the product is already in the cart its quantity is increased.
    /// </summary>
    public class AddCartItemCommand : IRequest<CartDTO>
    {
        public string ProductId { get; set; } = null!;
        public int Quantity { get; set; }
    }
}
