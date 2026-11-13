using Microsoft.Extensions.Logging;

namespace Shopii.Infrastructure.Auditing;

/// <summary>
/// Compile-time logging for the infrastructure audit writer. 
/// Event ID range 3000-3099 infrastructure
/// </summary>
internal static partial class AuditLoggerMessages
{
    /// <summary>
    /// Logs an error when an audit log cannot be written to the database.
    /// This is a critical failure that should be investigated.
    /// </summary>
    [LoggerMessage(
        EventId = 3000,
        EventName = "AuditWriteFailed",
        Level = LogLevel.Error,
        Message = "Failed to write audit log for {AuditAction} on {EntityType} {EntityId}")]
    public static partial void LogAuditWriteFailed(
        this ILogger logger,
        string auditAction,
        string entityType,
        string entityId,
        Exception exception);
}
