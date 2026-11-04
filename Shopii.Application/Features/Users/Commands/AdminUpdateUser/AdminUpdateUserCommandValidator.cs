using FluentValidation;

namespace Shopii.Application.Features.Users.Commands.AdminUpdateUser
{
    public class AdminUpdateUserCommandValidator
        : AbstractValidator<AdminUpdateUserCommand>
    {
        public AdminUpdateUserCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty();

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(255);

            RuleFor(x => x.RoleId)
                .GreaterThan(0);

            RuleFor(x => x.Password)
                .MinimumLength(8)
                .When(x => !string.IsNullOrWhiteSpace(x.Password));
        }
    }
}