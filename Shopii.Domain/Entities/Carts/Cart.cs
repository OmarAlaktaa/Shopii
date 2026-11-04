using System.ComponentModel.DataAnnotations;

namespace Shopii.Domain.Entities.Carts;

/// <summary>
/// A user's shopping cart. Every authenticated user has at most one cart;
/// the items are snapshotted into an <see cref="Shopii.Domain.Entities.Orders.Order"/>
/// at checkout time.
/// </summary>
public class Cart
{
    [Required]
    [Key]
    public int CartId { get; set; }

    [Required]
    public string UserId { get; set; } = null!;

    public List<CartItem> Items { get; set; } = new List<CartItem>();

    [Required]
    public DateTime CreatedAt { get; set; }

    // Stored as "UpdatingAt" in the database.
    public DateTime? UpdatedAt { get; set; }
}
