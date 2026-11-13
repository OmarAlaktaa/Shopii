using Microsoft.Extensions.Logging;

namespace Shopii.Application.Common.Logging;

/// <summary>
/// Compile-time logging for high-frequency application paths.
/// Event ID ranges:
///   1000-1099 authentication/users
///   1100-1199 orders
///   1200-1299 carts
///   1300-1399 products
/// </summary>
internal static partial class LogExtensions
{
    [LoggerMessage(
        EventId = 1000,
        EventName = "UserAuthenticated",
        Level = LogLevel.Information,
        Message = "User {UserId} authenticated successfully")]
    public static partial void LogUserAuthenticated(
        this ILogger logger,
        string userId);

    [LoggerMessage(
        EventId = 1001,
        EventName = "AuthenticationFailed",
        Level = LogLevel.Warning,
        Message = "Authentication failed for an unknown account or invalid credentials")]
    public static partial void LogAuthenticationFailed(this ILogger logger);

    [LoggerMessage(
        EventId = 1002,
        EventName = "AccountNotVerified",
        Level = LogLevel.Warning,
        Message = "Authentication rejected because user {UserId} is not verified")]
    public static partial void LogAccountNotVerified(
        this ILogger logger,
        string userId);

    [LoggerMessage(
        EventId = 1100,
        EventName = "OrderPlaced",
        Level = LogLevel.Information,
        Message = "Order {OrderId} placed by user {UserId} with {ItemCount} item(s) totaling {TotalAmount}")]
    public static partial void LogOrderPlaced(
        this ILogger logger,
        string orderId,
        string userId,
        int itemCount,
        decimal totalAmount);
}
