using Shopii.Domain.Entities.Users;

namespace Shopii.Application.Interfaces.Authentication
{
    public interface IPasswordHasher
    {
        string HashPassword(string password, out string salt);
        bool VerifyPassword(string password, string salt, string storedHash);
    }
}
