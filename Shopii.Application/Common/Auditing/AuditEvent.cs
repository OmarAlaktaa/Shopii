namespace Shopii.Application.Common.Auditing;

/// <summary>
/// A single audit event. The action and entity type should be values from
/// <see cref="AuditAction"/> and <see cref="AuditEntityTypes"/>.
/// </summary>
public sealed record AuditEvent(
    AuditAction Action,
    string EntityType,
    string? EntityId = null,
    string? ActorUserId = null,
    AuditDetails? Details = null);
