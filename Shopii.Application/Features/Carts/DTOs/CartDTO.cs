namespace Shopii.Application.Features.Carts.DTOs
{
    public class CartDTO
    {
        public int CartId { get; set; }
        public string UserId { get; set; } = null!;
        public List<CartItemDTO> Items { get; set; } = new List<CartItemDTO>();

        // Sum of quantity × current product price across all items.
        public decimal TotalAmount { get; set; }
    }

    public class CartItemDTO
    {
        public int CartItem { get; set; }
        public string ProductId { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }

        // Current product price (carts hold live prices; the snapshot happens
        // when the order is placed).
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
