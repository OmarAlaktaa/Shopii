using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shopii.Application;
using Shopii.Application.Common.Behaviors;
using Shopii.Application.Common.Models;
using Shopii.Application.Features.Orders.Queries.GetAllOrders;
using Shopii.Application.Features.Orders.Queries.GetMyOrders;
using Shopii.Application.Features.Products.Queries.GetAllProducts;
using Shopii.Application.Features.Users.DTOs;
using Shopii.Application.Features.Users.Queries.GetAllUsers;
using Xunit;

namespace Shopii.Tests
{
    public class PaginationPipelineTests
    {
        [Fact]
        public void ApplicationRegistersEveryPagedQueryValidator()
        {
            var services = new ServiceCollection();
            services.AddApplication();

            using var provider = services.BuildServiceProvider();

            Assert.NotNull(provider.GetService<IValidator<GetAllUsersQuery>>());
            Assert.NotNull(provider.GetService<IValidator<GetAllOrdersQuery>>());
            Assert.NotNull(provider.GetService<IValidator<GetAllProductsQuery>>());
            Assert.NotNull(provider.GetService<IValidator<GetMyOrdersQuery>>());
        }

        [Fact]
        public async Task ValidationBehavior_RejectsInvalidPagination()
        {
            var behavior = new ValidationBehavior<GetAllUsersQuery, PagedResult<UserDTO>>(
                new IValidator<GetAllUsersQuery>[]
                {
                    new GetAllUsersQueryValidator()
                });

            await Assert.ThrowsAsync<Shopii.Application.Common.Exceptions.ValidationException>(
                () => behavior.Handle(
                    new GetAllUsersQuery(0, 500),
                    _ => Task.FromResult(
                        new PagedResult<UserDTO>(
                            Array.Empty<UserDTO>(), 0, 0, 0)),
                    CancellationToken.None));
        }
    }
}
