using Shopii.Application.Common.Auditing;

namespace Shopii.Application.Interfaces.Services;

/// <summary>
/// Writes security and business audit events. Implementations must be
/// best-effort: an audit failure must never break the caller's request.
/// </summary>
public interface IAuditLogger
{
    Task LogAsync(
        AuditEvent auditEvent,
        CancellationToken cancellationToken = default);
}
