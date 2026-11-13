using Shopii.API.Middleware;
using Shopii.Application.Interfaces.Services;

namespace Shopii.API.Services;

/// <summary>
/// Provides access to the correlation ID associated with the current HTTP context.
/// </summary>
public sealed class HttpCorrelationContext : ICorrelationContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCorrelationContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? CorrelationId =>
        _httpContextAccessor.HttpContext?.Items[CorrelationIdMiddleware.ItemKey]
            as string;
}
