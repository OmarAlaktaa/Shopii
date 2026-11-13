namespace Shopii.Application.Common.Auditing;

/// <summary>
/// Stable entity type names stored in <c>AuditLogs.EntityType</c>.
/// </summary>
public static class AuditEntityTypes
{
    public const string User = nameof(User);
    public const string Order = nameof(Order);
    public const string Product = nameof(Product);
}
