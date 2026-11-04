using MediatR;
using Shopii.Application.Features.Orders.DTOs;

namespace Shopii.Application.Features.Orders.Commands.UpdateOrderStatus
{
    /// <summary>
    /// Moves an order to a different status (lookup row from Order_Statuses).
    /// Reserved for admins; enforced by the "Admin" authorization policy and
    /// re-checked in the handler.
    /// </summary>
    public class UpdateOrderStatusCommand : IRequest<OrderDTO>
    {
        public string OrderId { get; set; } = null!;
        public int OrderStatusId { get; set; }
    }
}
