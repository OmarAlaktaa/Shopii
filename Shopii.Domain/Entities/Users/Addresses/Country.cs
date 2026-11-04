using System.ComponentModel.DataAnnotations;

public class Country
{
    [Required]
    [Key]
    public int CountryId { get; set; }
    [Required] 
    [StringLength(255)]
    public string Name { get; set; } = null!;
}