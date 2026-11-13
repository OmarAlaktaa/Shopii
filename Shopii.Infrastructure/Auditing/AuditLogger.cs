using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shopii.Application.Common.Auditing;
using Shopii.Application.Interfaces.Services;
using Shopii.Domain.Entities.Auditing;
using Shopii.Infrastructure.Persistence;

namespace Shopii.Infrastructure.Auditing;

/// <summary>
/// Best-effort audit writer. The business operation has already succeeded by
/// the time this runs, so a failed audit insert is logged and swallowed rather
/// than turning a successful request into a 500.
/// </summary>
public sealed class AuditLogger : IAuditLogger
{
    private static readonly TimeSpan WriteTimeout = TimeSpan.FromSeconds(2);

    private static readonly JsonSerializerOptions DetailsJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        MaxDepth = 4,
    };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICorrelationContext _correlationContext;
    private readonly ILogger<AuditLogger> _logger;

    public AuditLogger(
        IServiceScopeFactory scopeFactory,
        ICurrentUserService currentUserService,
        ICorrelationContext correlationContext,
        ILogger<AuditLogger> logger)
    {
        _scopeFactory = scopeFactory;
        _currentUserService = currentUserService;
        _correlationContext = correlationContext;
        _logger = logger;
    }

    // This logs to database.
    public async Task LogAsync(
        AuditEvent auditEvent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entry = new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                ActorUserId = auditEvent.ActorUserId
                    ?? _currentUserService.UserId,
                Action = auditEvent.Action.ToString(),
                EntityType = auditEvent.EntityType,
                EntityId = auditEvent.EntityId,
                CorrelationId = _correlationContext.CorrelationId,
                Details = auditEvent.Details is null
                    ? null
                    : JsonSerializer.Serialize(
                        auditEvent.Details.Values,
                        DetailsJsonOptions),
            };

            // Use a dedicated scope so a failed audit insert cannot leave the
            // request's DbContext in a partially tracked state.
            await using var scope = _scopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider
                .GetRequiredService<ShopiiDbContext>();

            context.AuditLogs.Add(entry);

            // Audit writes are best-effort but should survive a client disconnect.
            // Use a short independent timeout rather than the request cancellation token.
            using var timeout = new CancellationTokenSource(WriteTimeout);
            await context.SaveChangesAsync(timeout.Token);
        }
        catch (Exception ex)
        {
            _logger.LogAuditWriteFailed(
                auditEvent.Action.ToString(),
                auditEvent.EntityType,
                auditEvent.EntityId ?? string.Empty, ex);
        }
    }
}
