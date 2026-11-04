using MediatR;
using Shopii.Application.Features.Users.DTOs;

namespace Shopii.Application.Features.Users.Commands.UpdateOwnProfile
{
    public class UpdateOwnProfileCommand : IRequest<UserDTO>
    {
        // A client-supplied identifier is never trusted. The handler resolves
        // the target account from the authenticated identity; this property is
        // retained only so a stray value cannot widen the operation's scope.
        public string? UserId { get; set; }

        public string Email { get; set; } = null!;

        // Optional: the password is only changed when a value is supplied.
        public string? Password { get; set; }
    }
}