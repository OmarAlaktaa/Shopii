namespace Shopii.API.Models
{
    // Request body for PUT /api/orders/{id}/status.
    // The order id comes from the route only.
    public class UpdateOrderStatusRequest
    {
        public int OrderStatusId { get; set; }
    }
}
