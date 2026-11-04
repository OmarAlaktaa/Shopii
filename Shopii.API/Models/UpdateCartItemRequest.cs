namespace Shopii.API.Models
{
    // Request body for PUT /api/carts/{productId}.
    // The product id comes from the route only, so it is deliberately
    // not part of the body (it would otherwise be demanded twice).
    public class UpdateCartItemRequest
    {
        public int Quantity { get; set; }
    }
}
