using Shopii.Application.Interfaces.Services;

namespace Shopii.Tests.Fakes
{
    public sealed class FakeCurrentUserService : ICurrentUserService
    {
        public string? UserId { get; set; }
        public string? Role { get; set; }
        public bool IsAdmin { get; set; }
    }
}