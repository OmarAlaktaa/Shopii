using FluentValidation;

namespace Shopii.Application.Features.Carts.Commands.RemoveCartItem
{
    public class RemoveCartItemCommandValidator
        : AbstractValidator<RemoveCartItemCommand>
    {
        public RemoveCartItemCommandValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty();
        }
    }
}
