using Shopii.Domain.Entities.Products;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopii.Domain.Entities.Carts;

public class CartItem
{
    [Required]
    [Key]
    public int Item { get; set; }

    [Required]
    [ForeignKey(nameof(Cart))]
    public int CartId { get; set; }
    public Cart Cart { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Product))]
    public string ProductId { get; set; } = null!;
    public Product Product { get; set; } = null!;

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
