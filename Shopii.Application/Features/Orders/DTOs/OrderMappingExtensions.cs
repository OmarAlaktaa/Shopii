using Shopii.Domain.Entities.Orders;

namespace Shopii.Application.Features.Orders.DTOs
{
    public static class OrderMappingExtensions
    {
        public static OrderDTO ToDTO(this Order order)
        {
            var items = order.OrderItems
                .Select(item => new OrderItemDTO
                {
                    OrderItemId = item.OrderItemId,
                    ProductId = item.ProductId,
                    // A soft-deleted product keeps its order history intact; its
                    // name simply shows as unknown in old orders.
                    ProductName = item.Product?.Name ?? "(unavailable)",
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.UnitPrice * item.Quantity
                })
                .ToList();

            return new OrderDTO
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                OrderStatusId = order.OrderStatusId,
                StatusTitle = order.Status?.Title ?? string.Empty,
                Items = items,
                Payment = order.Payment is null
                    ? null!
                    : new PaymentDTO
                    {
                        PaymentId = order.Payment.PaymentId,
                        Amount = order.Payment.Amount,
                        PaymentMethodId = order.Payment.PaymentMethodId,
                        PaymentMethodTitle = order.Payment.PaymentMethod?.Title ?? string.Empty,
                        PaymentStatusId = order.Payment.PaymentStatusId,
                        PaymentStatusTitle = order.Payment.Status?.Title ?? string.Empty,
                        CreatedAt = order.Payment.CreatedAt
                    },
                TotalAmount = items.Sum(item => item.LineTotal),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }
    }
}
