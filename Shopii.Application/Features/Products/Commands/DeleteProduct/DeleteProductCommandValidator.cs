using FluentValidation;

namespace Shopii.Application.Features.Products.Commands.DeleteProduct
{
    public class DeleteProductCommandValidator
        : AbstractValidator<DeleteProductCommand>
    {
        public DeleteProductCommandValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty();
        }
    }
}
