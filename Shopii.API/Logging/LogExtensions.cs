namespace Shopii.API.Logging;

/// <summary>
/// Compile-time logging for API middleware.
/// Event ID ranges: 2000-2099 middleware / infrastructure
/// </summary>
internal static partial class LogExtensions
{
    /// <summary>
    /// Logs a rejected request at debug level.
    /// </summary>
    [LoggerMessage(
        EventId = 2000,
        EventName = "RequestRejected",
        Level = LogLevel.Debug,
        Message = "Request rejected with {StatusCode} for {RequestMethod} {RequestPath}")]
    public static partial void LogRequestRejected(
        this ILogger logger,
        int statusCode,
        string requestMethod,
        string requestPath,
        Exception exception);

    /// <summary>
    /// Logs a database update conflict for a request.
    /// </summary>
    [LoggerMessage(
        EventId = 2001,
        EventName = "RequestConflict",
        Level = LogLevel.Warning,
        Message = "Database update conflict for {RequestMethod} {RequestPath}")]
    public static partial void LogRequestConflict(
        this ILogger logger,
        string requestMethod,
        string requestPath,
        Exception exception);

    [LoggerMessage(
        EventId = 2002,
        EventName = "UnhandledException",
        Level = LogLevel.Error,
        Message = "Unhandled exception for {RequestMethod} {RequestPath}")]
    public static partial void LogUnhandledException(
        this ILogger logger,
        string requestMethod,
        string requestPath,
        Exception exception);
}
