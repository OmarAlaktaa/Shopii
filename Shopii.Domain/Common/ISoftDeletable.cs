namespace Shopii.Domain.Common
{
    /// <summary>
    /// Marks an entity whose delete operation is a soft delete: instead of
    /// physically removing the row, it is kept and stamped with who/when it
    /// was deleted. Such entities are automatically filtered out of normal
    /// queries and are rewritten from a remove into an update on save.
    /// </summary>
    public interface ISoftDeletable
    {
        DateTime? DeletedAt { get; set; }
        string? DeletedByUserId { get; set; }
    }
}
