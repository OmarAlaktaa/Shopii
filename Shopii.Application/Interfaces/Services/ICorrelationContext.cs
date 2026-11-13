namespace Shopii.Application.Interfaces.Services;

/// <summary>
/// Provides the current request correlation id to application and
/// infrastructure services without taking a dependency on ASP.NET Core.
/// </summary>
public interface ICorrelationContext
{
    string? CorrelationId { get; }
}
