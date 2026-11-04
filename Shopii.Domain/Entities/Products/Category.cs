using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopii.Domain.Entities.Products;

public class Category
{
    [Required]
    [Key]
    public int CategoryId { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [ForeignKey(nameof(ParentCategory))]
    public int? ParentCategoryId { get; set; }

    public Category? ParentCategory { get; set; }
}
