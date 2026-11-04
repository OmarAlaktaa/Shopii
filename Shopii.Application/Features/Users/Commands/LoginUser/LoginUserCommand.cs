using MediatR;

namespace Shopii.Application.Features.Users.Commands.LoginUser
{
    public class LoginUserCommand : IRequest<string> // Returns a JWT Token as string
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
