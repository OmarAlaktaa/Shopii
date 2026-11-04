using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopii.Domain.Entities.Products;

public class ProductImage
{
    [Required]
    [Key]
    public int ImageId { get; set; }

    [Required]
    [ForeignKey(nameof(Product))]
    public string ProductId { get; set; } = null!;

    public Product Product { get; set; } = null!;

    public string ImagePath { get; set; } = null!;
}
