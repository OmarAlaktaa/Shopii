namespace Shopii.API.Models
{
    // Request body for self-service profile updates.
    // Privileged fields (RoleId, IsVerified) are deliberately absent.
    public class UpdateOwnProfileRequest
    {
        public string Email { get; set; } = null!;

        // Optional: the password is only changed when a value is supplied.
        public string? Password { get; set; }
    }
}