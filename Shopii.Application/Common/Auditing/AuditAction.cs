namespace Shopii.Application.Common.Auditing;

/// <summary>
/// Stable action names stored in <c>AuditLogs.Action</c>. The persisted value is
/// the enum member name, never the numeric value.
/// </summary>
public enum AuditAction
{
    // Authentication
    UserRegistered,
    LoginSucceeded,
    LoginFailed,
    LoginRejected,

    // Users
    UsersListViewed,
    UserProfileViewed,
    UserUpdated,
    OwnProfileUpdated,
    UserDeleted,

    // Orders
    OrdersListViewed,
    OrderViewed,
    OrderCreated,
    OrderUpdated,
    OrderStatusChanged,
    OrderArchived,

    // Products
    ProductCreated,
    ProductUpdated,
    ProductDeleted,
}
