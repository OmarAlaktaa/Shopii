using System.ComponentModel.DataAnnotations;

namespace Shopii.Domain.Entities.Products;

public class Brand
{
    [Required]
    [Key]
    public int BrandId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = null!;
}
