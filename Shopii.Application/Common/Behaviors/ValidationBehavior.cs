using FluentValidation;
using MediatR;

namespace Shopii.Application.Common.Behaviors
{
    /// <summary>
    /// Runs every registered <see cref="IValidator{T}"/> for the incoming request
    /// before the handler executes. When validation fails, the collected messages
    /// are thrown as a <see cref="Shopii.Application.Common.Exceptions.ValidationException"/>
    /// which the API layer translates into an HTTP 400 response.
    /// </summary>
    public class ValidationBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // No validators registered for this request: nothing to check.
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            var failures = _validators
                .Select(validator => validator.Validate(context))
                .SelectMany(result => result.Errors)
                .Where(failure => failure is not null)
                .ToList();

            if (failures.Count != 0)
            {
                var message = string.Join(
                    "; ",
                    failures.Select(failure => failure.ErrorMessage));

                throw new Shopii.Application.Common.Exceptions.ValidationException(message);
            }

            return await next();
        }
    }
}
