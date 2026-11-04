namespace Shopii.API.Models
{
    // Request body for PUT /api/orders/{id}.
    // The order id comes from the route only, so it is deliberately
    // not part of the body (it would otherwise be demanded twice).
    public class UpdateOrderRequest
    {
        public int PaymentMethodId { get; set; }
        public List<UpdateOrderItemRequest> Items { get; set; } = new List<UpdateOrderItemRequest>();
    }

    public class UpdateOrderItemRequest
    {
        public string ProductId { get; set; } = null!;
        public int Quantity { get; set; }
    }
}
