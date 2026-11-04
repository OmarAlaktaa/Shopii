using FluentValidation;

namespace Shopii.Application.Features.Carts.Commands.UpdateCartItemQuantity
{
    public class UpdateCartItemQuantityCommandValidator
        : AbstractValidator<UpdateCartItemQuantityCommand>
    {
        public UpdateCartItemQuantityCommandValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty();

            // Quantities are absolute; zero (or less) is not a delete, it is an
            // invalid request. Removal goes through DELETE /api/carts/{productId}.
            RuleFor(x => x.Quantity).GreaterThan(0);
        }
    }
}
