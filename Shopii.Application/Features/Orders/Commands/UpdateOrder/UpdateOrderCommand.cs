using MediatR;
using Shopii.Application.Features.Orders.DTOs;

namespace Shopii.Application.Features.Orders.Commands.UpdateOrder
{
    /// <summary>
    /// Replaces the line items and payment method of an existing order. The
    /// order is only editable while its status is still "Pending"; its status
    /// is changed through <see cref="UpdateOrderStatus.UpdateOrderStatusCommand"/>.
    /// </summary>
    public class UpdateOrderCommand : IRequest<OrderDTO>
    {
        public string OrderId { get; set; } = null!;
        public int PaymentMethodId { get; set; }
        public List<UpdateOrderItemDTO> Items { get; set; } = new List<UpdateOrderItemDTO>();
    }

    public class UpdateOrderItemDTO
    {
        public string ProductId { get; set; } = null!;
        public int Quantity { get; set; }
    }
}
