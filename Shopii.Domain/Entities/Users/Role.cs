using System.ComponentModel.DataAnnotations;

public class Role
{
    [Required]
    [Key]
    public int RoleId { get; set; }
    public string RoleTitle { get; set; } = null!;
}
