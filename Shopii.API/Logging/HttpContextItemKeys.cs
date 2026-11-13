namespace Shopii.API.Logging;

public static class HttpContextItemKeys
{
    /// <summary>
    /// Set by the exception-handling middleware when it has already written a
    /// detailed Warning/Error entry for the current request.
    /// </summary>
    public const string ExceptionLogged = "ExceptionLogged";
}
