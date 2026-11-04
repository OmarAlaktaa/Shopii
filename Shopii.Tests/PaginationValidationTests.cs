using FluentValidation;
using Shopii.Application.Common.Models;
using Shopii.Application.Features.Orders.Queries.GetAllOrders;
using Shopii.Application.Features.Orders.Queries.GetMyOrders;
using Shopii.Application.Features.Products.Queries.GetAllProducts;
using Shopii.Application.Features.Users.Queries.GetAllUsers;
using Xunit;

namespace Shopii.Tests
{
    public class PaginationValidationTests
    {
        [Fact]
        public void PaginatedQueries_RejectInvalidPageNumberOrPageSize()
        {
            AssertPagination(
                new GetAllUsersQueryValidator(),
                (pageNumber, pageSize) => new GetAllUsersQuery(pageNumber, pageSize));

            AssertPagination(
                new GetAllOrdersQueryValidator(),
                (pageNumber, pageSize) => new GetAllOrdersQuery(pageNumber, pageSize));

            AssertPagination(
                new GetAllProductsQueryValidator(),
                (pageNumber, pageSize) => new GetAllProductsQuery(pageNumber, pageSize));

            AssertPagination(
                new GetMyOrdersQueryValidator(),
                (pageNumber, pageSize) => new GetMyOrdersQuery(pageNumber, pageSize));
        }

        private static void AssertPagination<TRequest>(
            IValidator<TRequest> validator,
            Func<int, int, TRequest> create)
            where TRequest : IPagedRequest
        {
            // PageNumber must be >= 1.
            var pageNumberResult = validator.Validate(create(0, 10));
            Assert.False(pageNumberResult.IsValid);
            Assert.Contains(
                pageNumberResult.Errors,
                failure => failure.ErrorMessage == "Page number must be at least 1.");

            // PageSize must be between 1 and 100.
            var pageSizeResult = validator.Validate(create(1, 500));
            Assert.False(pageSizeResult.IsValid);
            Assert.Contains(
                pageSizeResult.Errors,
                failure => failure.ErrorMessage == "Page size must be between 1 and 100.");

            Assert.False(validator.Validate(create(1, 0)).IsValid);
            Assert.False(validator.Validate(create(-1, -5)).IsValid);

            // Bounds are inclusive.
            Assert.True(validator.Validate(create(1, 1)).IsValid);
            Assert.True(validator.Validate(create(1, 100)).IsValid);
            Assert.True(validator.Validate(create(25, 50)).IsValid);
        }
    }
}
