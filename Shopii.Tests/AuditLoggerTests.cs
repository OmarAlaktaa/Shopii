using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shopii.Application.Common.Auditing;
using Shopii.Application.Interfaces.Services;
using Shopii.Infrastructure.Auditing;
using Shopii.Infrastructure.Persistence;
using Xunit;

namespace Shopii.Tests
{
    public class AuditLoggerTests
    {
        [Fact]
        public async Task LogAsync_WritesActorEntityAndCorrelation()
        {
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var services = new ServiceCollection();
            services.AddDbContext<ShopiiDbContext>(
                options => options.UseSqlite(connection));

            await using var provider = services.BuildServiceProvider();

            await using (var setupScope = provider.CreateAsyncScope())
            {
                var setupContext = setupScope.ServiceProvider
                    .GetRequiredService<ShopiiDbContext>();
                await setupContext.Database.EnsureCreatedAsync();
            }

            var logger = new AuditLogger(
                provider.GetRequiredService<IServiceScopeFactory>(),
                new FakeCurrentUserService("admin-1"),
                new FakeCorrelationContext("corr-123"),
                NullLogger<AuditLogger>.Instance);

            await logger.LogAsync(
                new AuditEvent(
                    AuditAction.UserUpdated,
                    AuditEntityTypes.User,
                    "target-1",
                    Details: AuditDetails.Create(
                        ("RoleId", 2),
                        ("Email", "person@example.com"))));

            await using var verifyScope = provider.CreateAsyncScope();
            var context = verifyScope.ServiceProvider
                .GetRequiredService<ShopiiDbContext>();
            var entry = await context.AuditLogs.SingleAsync();

            Assert.Equal("admin-1", entry.ActorUserId);
            Assert.Equal(AuditAction.UserUpdated.ToString(), entry.Action);
            Assert.Equal(AuditEntityTypes.User, entry.EntityType);
            Assert.Equal("target-1", entry.EntityId);
            Assert.Equal("corr-123", entry.CorrelationId);
            Assert.Contains("\"roleId\":2", entry.Details);
            Assert.Contains("\"email\":\"[REDACTED]\"", entry.Details);
            Assert.DoesNotContain("person@example.com", entry.Details);
        }

        [Fact]
        public async Task LogAsync_SwallowsPersistenceFailures()
        {
            var logger = new AuditLogger(
                new ThrowingScopeFactory(),
                new FakeCurrentUserService(null),
                new FakeCorrelationContext(null),
                NullLogger<AuditLogger>.Instance);

            // The scope factory throws before SaveChanges can run; the audit
            // writer must not surface that failure to the caller.
            await logger.LogAsync(
                new AuditEvent(AuditAction.UserUpdated, "TestEntity"));
        }

        private sealed class FakeCurrentUserService : ICurrentUserService
        {
            public FakeCurrentUserService(string? userId)
            {
                UserId = userId;
            }

            public string? UserId { get; }

            public string? Role => null;

            public bool IsAdmin => false;
        }

        private sealed class FakeCorrelationContext : ICorrelationContext
        {
            public FakeCorrelationContext(string? correlationId)
            {
                CorrelationId = correlationId;
            }

            public string? CorrelationId { get; }
        }

        private sealed class ThrowingScopeFactory : IServiceScopeFactory
        {
            public IServiceScope CreateScope()
            {
                throw new InvalidOperationException("Audit database unavailable.");
            }
        }
    }
}
