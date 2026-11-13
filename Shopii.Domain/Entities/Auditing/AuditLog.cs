namespace Shopii.Domain.Entities.Auditing;

/// <summary>
/// Append-only record of security- and business-sensitive actions. 
/// Audit rows intentionally contain identifiers and coarse details only, 
/// never passwords, tokens, email addresses, or request bodies.
/// </summary>
public class AuditLog
{
    public long AuditLogId { get; set; }

    public DateTime Timestamp { get; set; }

    /// <summary>
    /// The authenticated actor, or null for an anonymous action such as a
    /// failed login.
    /// </summary>
    public string? ActorUserId { get; set; }

    public string Action { get; set; } = null!;

    public string EntityType { get; set; } = null!;

    public string? EntityId { get; set; }

    public string? CorrelationId { get; set; }

    /// <summary>
    /// Small JSON payload with non-sensitive context (for example the new
    /// order status). Never store request bodies or PII here.
    /// </summary>
    public string? Details { get; set; }
}
