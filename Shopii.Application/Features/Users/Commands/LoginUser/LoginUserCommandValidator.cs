using FluentValidation;

namespace Shopii.Application.Features.Users.Commands.LoginUser
{
    public class LoginUserCommandValidator
        : AbstractValidator<LoginUserCommand>
    {
        public LoginUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }
}
