using FluentValidation;

namespace Shopii.Application.Features.Users.Commands.UpdateOwnProfile
{
    public class UpdateOwnProfileCommandValidator
        : AbstractValidator<UpdateOwnProfileCommand>
    {
        public UpdateOwnProfileCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(255);

            RuleFor(x => x.Password)
                .MinimumLength(8)
                .When(x => !string.IsNullOrWhiteSpace(x.Password));
        }
    }
}