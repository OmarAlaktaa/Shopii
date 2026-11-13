using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shopii.API.Options;
using Shopii.Infrastructure.Persistence;

namespace Shopii.API.Services;

/// <summary>
/// Deletes audit rows older than the configured retention window. Audit data
/// lives in SQLite rather than Seq, so it needs its own cleanup policy.
/// </summary>
public sealed class AuditLogRetentionService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<AuditOptions> _options;
    private readonly ILogger<AuditLogRetentionService> _logger;

    public AuditLogRetentionService(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<AuditOptions> options,
        ILogger<AuditLogRetentionService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DeleteExpiredAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // Retention is housekeeping; never let it take the host down.
                _logger.LogError(ex, "Audit log retention cleanup failed.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task DeleteExpiredAsync(CancellationToken cancellationToken)
    {
        // Startup validation guarantees this value is configured. Reading it
        // through IOptionsMonitor keeps appsettings reloads effective without a
        // code change or restart.
        var retentionDays = _options.CurrentValue.RetentionDays
            ?? throw new InvalidOperationException(
                "Audit:RetentionDays must be configured.");

        if (retentionDays <= 0)
        {
            _logger.LogInformation(
                "Audit log retention is disabled (Audit:RetentionDays = {RetentionDays}).",
                retentionDays);
            return;
        }

        var cutoff = DateTime.UtcNow.AddDays(-retentionDays);

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<ShopiiDbContext>();

        var deleted = await context.AuditLogs
            .Where(a => a.Timestamp < cutoff)
            .ExecuteDeleteAsync(cancellationToken); // Note: EF Core 7.0+ feature; deletes in the database without loading entities into memory, so no need to call SaveChangesAsync() afterward.

        if (deleted > 0)
        {
            _logger.LogInformation(
                "Deleted {Count} audit log entries older than {Cutoff:O}.",
                deleted,
                cutoff);
        }
    }
}
