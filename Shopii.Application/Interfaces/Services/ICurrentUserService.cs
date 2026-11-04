namespace Shopii.Application.Interfaces.Services
{
    public interface ICurrentUserService
    {
        string? UserId { get; }

        /// <summary>
        /// Role title of the logged-in user (e.g. "Admin", "Customer"),
        /// taken from the JWT role claim. Null when unauthenticated.
        /// </summary>
        string? Role { get; }

        /// <summary>True when the logged-in user holds the "Admin" role.</summary>
        bool IsAdmin { get; }
    }
}
