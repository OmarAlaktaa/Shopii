using FluentValidation;
using Shopii.Application.Common.Models;

namespace Shopii.Application.Common.Validation
{
    /// <summary>
    /// Shared pagination rules for every <see cref="IPagedRequest"/>.
    ///
    /// Validation runs through <c>ValidationBehavior</c>, so invalid values are
    /// rejected before a handler reaches the repository. This replaces the
    /// per-handler <see cref="ArgumentOutOfRangeException"/> checks, which
    /// surfaced as HTTP 500 responses.
    /// </summary>
    public abstract class PagedRequestValidator<TRequest>
        : AbstractValidator<TRequest>
        where TRequest : IPagedRequest
    {
        protected PagedRequestValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page number must be at least 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100.");
        }
    }
}
