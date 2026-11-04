using Shopii.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopii.Domain.Entities.Products;

public class Product : BaseEntity
{
    [Required]
    [Key]
    public string ProductId { get; set; } = null!;
    public string Name { get; set; } = null!;


    [Required]
    [ForeignKey(nameof(Brand))]
    public int BrandId { get; set; }
    public Brand? Brand { get; set; }
    
    [Required]
    [ForeignKey(nameof(Category))]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public decimal CostPrice { get; set; }
    public decimal SoldPrice { get; set; }
    public string Description { get; set; } = null!;

    public List<ProductImage>? ProductImages { get; set; }

    public List<ProductVariant>? Variants { get; set; }
}
