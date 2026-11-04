using System.ComponentModel.DataAnnotations;

namespace Shopii.Domain.Entities.Payments;

public class PaymentMethod
{
    [Required]
    [Key]
    public int PaymentMethodId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;
}
