using System.ComponentModel.DataAnnotations;

namespace Shopii.Domain.Entities.Orders;

public class OrderStatus
{
    [Required]
    [Key]
    public int OrderStatusId { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = null!;
}
