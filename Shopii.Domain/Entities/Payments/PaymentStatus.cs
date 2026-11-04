using System.ComponentModel.DataAnnotations;

namespace Shopii.Domain.Entities.Payments;

public class PaymentStatus
{
    [Required]
    [Key]
    public int PaymentStatusId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;
}
