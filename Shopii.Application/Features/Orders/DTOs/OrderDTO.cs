namespace Shopii.Application.Features.Orders.DTOs
{
    public class OrderDTO
    {
        public string OrderId { get; set; } = null!;
        public string UserId { get; set; } = null!;

        public int OrderStatusId { get; set; }
        public string StatusTitle { get; set; } = null!;

        public List<OrderItemDTO> Items { get; set; } = new List<OrderItemDTO>();
        public PaymentDTO Payment { get; set; } = null!;

        // Sum of quantity × unit price across all line items.
        public decimal TotalAmount { get; set; }

        // Stored as "OrderedAt" in the database.
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
