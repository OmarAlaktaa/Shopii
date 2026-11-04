using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopii.Domain.Entities.Products;

public class ProductVariant
{
    [Required]
    [Key]
    public int ProductVariantId { get; set; }

    [Required]
    [ForeignKey(nameof(Product))]
    public string ProductId { get; set; } = null!;

    public Product Product { get; set; } = null!;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }
}
