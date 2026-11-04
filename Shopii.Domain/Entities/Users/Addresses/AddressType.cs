using System.ComponentModel.DataAnnotations;

public class AddressType
{
    [Required]
    [Key]
    public int AddressTypeId { get; set; }
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = null!;
}