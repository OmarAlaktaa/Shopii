using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shopii.Domain.Entities.Users;

public class Address
{
    [Required]
    [Key]
    public int AddressId { get; set; }
    
    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Country))]
    public int CountryId { get; set; }
    public Country Country { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(City))]
    public int CityId { get; set; }
    public City City { get; set; } = null!;

    public string? Street { get; set; } 
    public string? ZipCode { get; set; }


    [Required]
    [ForeignKey(nameof(AddressType))]
    public int AddressTypeId { get; set; }
    public AddressType AddressType { get; set; } = null!;
    
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}
