namespace Shopii.API.Models
{
    // Request body for PUT /api/users/{id}.
    // The user id comes from the route only, so it is deliberately
    // not part of the body (it would otherwise be demanded twice).
    public class UpdateUserRequest
    {
        public string Email { get; set; } = null!;
        public int RoleId { get; set; }
        public bool IsVerified { get; set; }

        // Optional: the password is only changed when a value is supplied.
        public string? Password { get; set; }
    }
}
