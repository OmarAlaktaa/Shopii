using Shopii.Application.Common.Auditing;
using Xunit;

namespace Shopii.Tests
{
    public class AuditDetailsTests
    {
        [Fact]
        public void SensitiveKeys_AreRedacted()
        {
            var details = AuditDetails.Create(
                ("Email", "person@example.com"),
                ("Password", "secret"),
                ("UserEmail", "person@example.com"),
                ("CardNumber", "4111111111111111"),
                ("PasswordChanged", true),
                ("RoleId", 2));

            Assert.Equal("[REDACTED]", details.Values["Email"]);
            Assert.Equal("[REDACTED]", details.Values["Password"]);
            Assert.Equal("[REDACTED]", details.Values["UserEmail"]);
            Assert.Equal("[REDACTED]", details.Values["CardNumber"]);
            Assert.Equal(true, details.Values["PasswordChanged"]);
            Assert.Equal(2, details.Values["RoleId"]);
        }

        [Fact]
        public void UnsupportedObjects_AreNotSerialized()
        {
            var details = AuditDetails.Create(
                ("User", new { Email = "person@example.com" }));

            Assert.Equal("[UNSUPPORTED]", details.Values["User"]);
        }

        [Fact]
        public void LongStrings_AreTruncated()
        {
            var details = AuditDetails.Create(
                ("Note", new string('x', AuditDetails.MaxStringLength + 10)));

            var text = Assert.IsType<string>(details.Values["Note"]);

            Assert.StartsWith(
                new string('x', AuditDetails.MaxStringLength),
                text);
            Assert.EndsWith("...", text);
        }

        [Fact]
        public void TooManyKeys_AreRejected()
        {
            var details = new AuditDetails();

            for (var i = 0; i < AuditDetails.MaxKeys; i++)
            {
                details.Add($"Key{i}", i);
            }

            Assert.Throws<InvalidOperationException>(
                () => details.Add("OneTooMany", 1));
        }
    }
}
