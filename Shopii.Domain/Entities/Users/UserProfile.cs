using System.ComponentModel.DataAnnotations;

public class UserProfile
{
    [Required]
    [Key]
    public string UserId { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Phone { get; set; }
    public DateTime DateOfBirth { get; set; }
    public bool Gender { get; set; } // true if Male, false if Female
    [Url(ErrorMessage = "Invalid URL")]
    public string? AvatarUrl { get; set; }
}
