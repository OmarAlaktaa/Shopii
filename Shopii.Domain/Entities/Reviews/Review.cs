using Shopii.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shopii.Domain.Entities.Users;
using Shopii.Domain.Entities.Products;

public class Review : BaseEntity
{
    [Required]
    [Key]
    public string ReviewId { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Product))]
    public string ProductId { get; set; } = null!;
    public Product Product { get; set; } = null!;

    [Range(1, 5)]
    public float Rating { get; set; }

    [MaxLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
    public string? Comment { get; set; }
}

