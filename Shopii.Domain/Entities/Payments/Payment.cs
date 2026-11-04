using Shopii.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopii.Domain.Entities.Payments;

public class Payment : BaseEntity
{
    [Required]
    [Key]
    public int PaymentId { get; set; }
    public decimal Amount { get; set; }

    [Required]
    [ForeignKey(nameof(PaymentMethod))]
    public int PaymentMethodId { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Status))]
    public int PaymentStatusId { get; set; }
    public PaymentStatus Status { get; set; } = null!;
}
