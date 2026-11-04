using Shopii.Application.Interfaces.Authentication;

namespace Shopii.Tests.Fakes
{
    public sealed class FakePasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password, out string salt)
        {
            salt = "test-salt";
            return "hashed-" + password;
        }

        public bool VerifyPassword(string hashedPassword, string salt, string providedPassword) =>
            hashedPassword == "hashed-" + providedPassword;
    }
}