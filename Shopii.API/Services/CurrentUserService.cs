using System.Security.Claims;
using Shopii.Application.Interfaces.Services;

namespace Shopii.API.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        // Keep in sync with the claim added by TokenService.GenerateToken.
        private const string AdminRoleTitle = "Admin";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId =>
            _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public string? Role =>
            _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.Role)?.Value;

        public bool IsAdmin =>
            string.Equals(Role, AdminRoleTitle, StringComparison.OrdinalIgnoreCase);
    }
}
