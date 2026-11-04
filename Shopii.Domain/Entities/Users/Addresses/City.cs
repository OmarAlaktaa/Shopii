using System.ComponentModel.DataAnnotations;

public class City
{
    [Required]
    [Key]
    public int CityId { get; set; }
    [Required]
    [MaxLength(255)]    
    public string Name { get; set; } = null!;
}
