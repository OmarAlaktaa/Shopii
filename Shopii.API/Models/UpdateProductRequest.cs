namespace Shopii.API.Models
{
    // Request body for PUT /api/products/{id}.
    // The product id comes from the route only, so it is deliberately
    // not part of the body (it would otherwise be demanded twice).
    public class UpdateProductRequest
    {
        public string Name { get; set; } = null!;
        public int BrandId { get; set; }
        public int CategoryId { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SoldPrice { get; set; }
        public string Description { get; set; } = null!;
    }
}
