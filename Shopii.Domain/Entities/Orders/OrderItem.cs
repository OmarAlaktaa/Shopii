using Shopii.Domain.Entities.Products;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopii.Domain.Entities.Orders;

public class OrderItem
{
    [Required]
    [Key]
    public int OrderItemId { get; set; }

    [Required]
    [ForeignKey(nameof(Product))]
    public string ProductId { get; set; } = null!;
    public Product Product { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Order))]
    public string OrderId { get; set; } = null!;
    public Order Order { get; set; } = null!;

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    // Price of a single unit at the moment the order was placed. It is a
    // snapshot: later product price changes never rewrite order history.
    public decimal UnitPrice { get; set; }
}
