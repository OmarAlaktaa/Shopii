using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Shopii.Application.Interfaces.Services;
using Shopii.Domain.Entities.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Shopii.Infrastructure.Authentication
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var secret = _configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new InvalidOperationException(
                    "Jwt:Key is missing or empty. Configure a Base64-encoded signing " +
                    "key of at least 32 bytes (256 bits) via user secrets " +
                    "(Development) or the Jwt__Key environment variable (Production).");
            }

            byte[] keyBytes;

            try
            {
                keyBytes = Convert.FromBase64String(secret);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException(
                    "Jwt:Key must be a valid Base64 string.", ex);
            }

            if (keyBytes.Length < 32)
            {
                throw new InvalidOperationException(
                    "Jwt:Key must decode to at least 32 bytes (256 bits) for " +
                    "HMAC-SHA256 signing.");
            }

            var issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT issuer is missing.");

            var audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "JWT audience is missing.");

            if (string.IsNullOrWhiteSpace(user.Email))
                throw new InvalidOperationException(
                    "User email is missing.");

            if (!int.TryParse(
                    _configuration["Jwt:AccessTokenMinutes"],
                    out var minutes) || minutes <= 0)
            {
                minutes = 15;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,
                    user.UserId.ToString()),

                new Claim(
                    ClaimTypes.Email,
                    user.Email),

                new Claim(
                    System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            };

            // Role claim: powers role-based policies such as "Admin".
            // Only added when the role is actually known.
            if (!string.IsNullOrWhiteSpace(user.Role?.RoleTitle))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role.RoleTitle));
            }

            var key = new SymmetricSecurityKey(keyBytes);

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(minutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}
