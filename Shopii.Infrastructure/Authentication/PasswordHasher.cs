using System.Security.Cryptography;
using Shopii.Application.Interfaces.Authentication;

namespace Shopii.Infrastructure.Authentication;

public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 4;      // 4 random bytes
    private const int HashSize = 32;      // 32-byte result
    private const int Iterations = 600_000;

    /// <summary>
    /// This function hashes a password using PBKDF2 with a
    /// random salt and returns the result in a specific format.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hashed password.</returns>
    /// <exception cref="ArgumentException"></exception>
    public string HashPassword(string password, out string salt)
    {
        byte[] saltBytes = RandomNumberGenerator.GetBytes(SaltSize);

        salt = Convert.ToBase64String(saltBytes);

        byte[] hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize
        );

        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(
    string password,
    string salt,
    string storedHash)
    {
        if (string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(salt) ||
            string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        try
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            // this 
            byte[] expectedHash = Convert.FromBase64String(storedHash);

            if (saltBytes.Length != SaltSize || expectedHash.Length != HashSize)
                return false;

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                saltBytes,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize
            );

            return CryptographicOperations.FixedTimeEquals(
                actualHash,
                expectedHash
            );
        }
        catch (FormatException)
        {
            return false;
        }
    }
}