using FluentValidation;

namespace Shopii.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommandValidator
        : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(255);

            RuleFor(x => x.BrandId)
                .GreaterThan(0);

            RuleFor(x => x.CategoryId)
                .GreaterThan(0);

            RuleFor(x => x.CostPrice)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.SoldPrice)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.Description)
                .NotEmpty();
        }
    }
}
