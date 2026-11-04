using MediatR;
using Shopii.Application.Features.Users.DTOs;

namespace Shopii.Application.Features.Users.Commands.RegisterUser
{
    public class RegisterUserCommand : IRequest<UserDTO>
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}