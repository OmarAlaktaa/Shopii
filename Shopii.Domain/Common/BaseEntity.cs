using System.ComponentModel.DataAnnotations;

namespace Shopii.Domain.Common
{
    public abstract class BaseEntity : ISoftDeletable
    {
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public string CreatedByUserId { get; set; } = null!;

        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedByUserId { get; set; }
        
        public DateTime? DeletedAt { get; set; }
        public string? DeletedByUserId { get; set; }
    }
}
