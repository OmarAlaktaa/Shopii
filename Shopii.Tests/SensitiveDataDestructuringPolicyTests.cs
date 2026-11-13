using Serilog;
using Serilog.Core;
using Serilog.Events;
using Shopii.API.Logging;
using Xunit;

namespace Shopii.Tests
{
    public class SensitiveDataDestructuringPolicyTests
    {
        [Fact]
        public void SensitiveProperties_AreRedacted()
        {
            var sink = new CollectingSink();

            using var logger = new LoggerConfiguration()
                .Destructure.With<SensitiveDataDestructuringPolicy>()
                .WriteTo.Sink(sink)
                .CreateLogger();

            logger.Information(
                "User {@User}",
                new
                {
                    UserId = "user-1",
                    Email = "person@example.com",
                    EmailAddress = "person@example.com",
                    Password = "plain-text-secret",
                    PasswordHash = "hash-value",
                    Salt = "salt-value",
                    Name = "Omar"
                });

            var structure = Assert.IsType<StructureValue>(
                sink.Events.Single().Properties["User"]);

            Assert.Equal("user-1", GetScalar(structure, "UserId"));
            Assert.Equal("Omar", GetScalar(structure, "Name"));
            Assert.Equal("[REDACTED]", GetScalar(structure, "Email"));
            Assert.Equal("[REDACTED]", GetScalar(structure, "EmailAddress"));
            Assert.Equal("[REDACTED]", GetScalar(structure, "Password"));
            Assert.Equal("[REDACTED]", GetScalar(structure, "PasswordHash"));
            Assert.Equal("[REDACTED]", GetScalar(structure, "Salt"));
        }

        [Fact]
        public void SensitiveDictionaryKeys_AreRedacted()
        {
            var sink = new CollectingSink();

            using var logger = new LoggerConfiguration()
                .Destructure.With<SensitiveDataDestructuringPolicy>()
                .WriteTo.Sink(sink)
                .CreateLogger();

            logger.Information(
                "Headers {@Headers}",
                new Dictionary<string, string>
                {
                    ["Authorization"] = "Bearer secret",
                    ["X-Trace"] = "trace-123"
                });

            var dictionary = Assert.IsType<DictionaryValue>(
                sink.Events.Single().Properties["Headers"]);

            Assert.Equal(
                "[REDACTED]",
                GetDictionaryScalar(dictionary, "Authorization"));
            Assert.Equal(
                "trace-123",
                GetDictionaryScalar(dictionary, "X-Trace"));
        }

        private static string? GetScalar(
            StructureValue structure,
            string propertyName)
        {
            var property = structure.Properties
                .Single(p => p.Name == propertyName);

            return (property.Value as ScalarValue)?.Value?.ToString();
        }

        private static string? GetDictionaryScalar(
            DictionaryValue dictionary,
            string key)
        {
            return dictionary.Elements.TryGetValue(
                new ScalarValue(key),
                out var value)
                ? (value as ScalarValue)?.Value?.ToString()
                : null;
        }

        private sealed class CollectingSink : ILogEventSink
        {
            public List<LogEvent> Events { get; } = new();

            public void Emit(LogEvent logEvent)
            {
                Events.Add(logEvent);
            }
        }
    }
}
