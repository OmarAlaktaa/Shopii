using FluentValidation;

namespace Shopii.Application.Features.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandValidator
        : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.PaymentMethodId).GreaterThan(0);
        }
    }
}
