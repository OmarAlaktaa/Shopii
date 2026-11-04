using Shopii.Domain.Entities.Users;

namespace Shopii.Application.Interfaces.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
