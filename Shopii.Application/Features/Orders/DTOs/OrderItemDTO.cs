namespace Shopii.Application.Features.Orders.DTOs
{
    public class OrderItemDTO
    {
        public int OrderItemId { get; set; }
        public string ProductId { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }

        // Price snapshot taken when the order was placed.
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
