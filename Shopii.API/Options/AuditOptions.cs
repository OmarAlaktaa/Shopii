namespace Shopii.API.Options;

/// <summary>
/// Configuration for the audit trail. Values come from the "Audit" section in
/// appsettings*.json or environment variables (for example
/// Audit__RetentionDays).
/// </summary>
public sealed class AuditOptions
{
    public const string SectionName = "Audit";

    /// <summary>
    /// Number of days to keep audit rows. Zero disables automatic cleanup.
    /// Null means the value was not configured, which is rejected at startup.
    /// </summary>
    public int? RetentionDays { get; set; }
}
