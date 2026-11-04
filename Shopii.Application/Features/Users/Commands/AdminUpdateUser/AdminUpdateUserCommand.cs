using MediatR;
using Shopii.Application.Features.Users.DTOs;

namespace Shopii.Application.Features.Users.Commands.AdminUpdateUser
{
    public class AdminUpdateUserCommand : IRequest<UserDTO>
    {
        public string UserId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int RoleId { get; set; }
        public bool IsVerified { get; set; }

        // Optional: the password is only changed when a value is supplied.
        public string? Password { get; set; }
    }
}