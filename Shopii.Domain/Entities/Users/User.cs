using Shopii.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopii.Domain.Entities.Users
{
    public class User : ISoftDeletable
    {
        [Required]
        [Key]
        public string UserId { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Salt { get; set; } = null!;

        [Required]
        [ForeignKey(nameof(Role))]
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public bool IsVerified { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // A user is never physically removed while other rows reference its id
        // (orders, reviews, carts, profiles, audit columns). Instead the delete
        // is stamped here and the user disappears from normal queries.
        public DateTime? DeletedAt { get; set; }
        public string? DeletedByUserId { get; set; }
    }
}