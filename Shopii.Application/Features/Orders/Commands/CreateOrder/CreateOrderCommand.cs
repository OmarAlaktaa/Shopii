using MediatR;
using Shopii.Application.Features.Orders.DTOs;

namespace Shopii.Application.Features.Orders.Commands.CreateOrder
{
    /// <summary>
    /// Places an order for the current user using the contents of their
    /// shopping cart. A payment row is created automatically (amount computed
    /// from the items, status "Pending") and the cart is emptied.
    /// </summary>
    public class CreateOrderCommand : IRequest<OrderDTO>
    {
        public int PaymentMethodId { get; set; }
    }
}
