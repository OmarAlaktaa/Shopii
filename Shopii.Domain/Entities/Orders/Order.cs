using Shopii.Domain.Common;
using Shopii.Domain.Entities.Payments;
using Shopii.Domain.Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopii.Domain.Entities.Orders;

public class Order : BaseEntity
{
    [Required]
    [Key]
    public string OrderId { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;

    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [Required]
    [ForeignKey(nameof(Payment))]
    public int PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Status))]
    public int OrderStatusId { get; set; }
    public OrderStatus Status { get; set; } = null!;
}
