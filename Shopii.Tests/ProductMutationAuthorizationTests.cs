using Shopii.Application.Features.Products.Commands.CreateProduct;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Application.Features.Products.Commands.DeleteProduct;
using Shopii.Application.Features.Products.Commands.UpdateProduct;
using Shopii.Application.Features.Products.DTOs;
using Shopii.Domain.Entities.Products;
using Shopii.Tests.Fakes;
using Xunit;

namespace Shopii.Tests
{
    public class ProductMutationAuthorizationTests
    {
        private sealed class ProductRepositoryDouble : IProductRepository
        {
            private readonly Dictionary<string, Product> _products = new();

            public void Seed(Product product) => _products[product.ProductId] = product;
            public int Count => _products.Count;
            public Product? Get(string productId) =>
                _products.TryGetValue(productId, out var product) ? product : null;

            public Task<Product?> GetByIdAsync(string productId, CancellationToken cancellationToken) =>
                Task.FromResult(Get(productId));

            public Task AddAsync(Product product, CancellationToken cancellationToken)
            {
                _products[product.ProductId] = product;
                return Task.CompletedTask;
            }

            public Task UpdateAsync(Product product, CancellationToken cancellationToken)
            {
                _products[product.ProductId] = product;
                return Task.CompletedTask;
            }

            public Task<bool> DeleteAsync(string productId, CancellationToken cancellationToken) =>
                Task.FromResult(_products.Remove(productId));

            public Task<(IReadOnlyList<Product> Products, int TotalCount)> GetPagedAsync(
                int pageNumber, int pageSize, CancellationToken cancellationToken = default) =>
                Task.FromResult(((IReadOnlyList<Product>)_products.Values.ToList(), _products.Count));
        }

        private static Product SeedProduct(string id, string ownerId) =>
            new Product
            {
                ProductId = id,
                Name = "Original",
                BrandId = 1,
                CategoryId = 1,
                CostPrice = 10m,
                SoldPrice = 20m,
                Description = "desc",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = ownerId
            };

        private static FakeCurrentUserService User(string id, string role) =>
            new FakeCurrentUserService
            {
                UserId = id,
                Role = role,
                IsAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
            };

        [Theory]
        [InlineData("Admin")]
        [InlineData("Manager")]
        [InlineData("Seller")]
        public async Task Create_AllowedRole_CreatesProduct(string role)
        {
            var repo = new ProductRepositoryDouble();
            var current = User("user-1", role);
            var handler = new CreateProductCommandHandler(repo, current);

            var result = await handler.Handle(new CreateProductCommand
            {
                Name = "New",
                BrandId = 1,
                CategoryId = 1,
                CostPrice = 5m,
                SoldPrice = 9m,
                Description = "new"
            }, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("user-1", repo.Get(result.ProductId)!.CreatedByUserId);
        }

        [Fact]
        public async Task Create_Customer_ThrowsUnauthorized_AndDoesNotMutate()
        {
            var repo = new ProductRepositoryDouble();
            var current = User("cust-1", "Customer");
            var handler = new CreateProductCommandHandler(repo, current);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                handler.Handle(new CreateProductCommand
                {
                    Name = "New",
                    BrandId = 1,
                    CategoryId = 1,
                    CostPrice = 5m,
                    SoldPrice = 9m,
                    Description = "new"
                }, CancellationToken.None));

            Assert.Equal(0, repo.Count);
        }

        [Fact]
        public async Task Create_Unauthenticated_ThrowsUnauthorized()
        {
            var repo = new ProductRepositoryDouble();
            var current = new FakeCurrentUserService { UserId = null };
            var handler = new CreateProductCommandHandler(repo, current);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                handler.Handle(new CreateProductCommand
                {
                    Name = "New",
                    BrandId = 1,
                    CategoryId = 1,
                    CostPrice = 5m,
                    SoldPrice = 9m,
                    Description = "new"
                }, CancellationToken.None));
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("Manager")]
        public async Task Update_AllowedUnrestrictedRole_UpdatesPrices(string role)
        {
            var repo = new ProductRepositoryDouble();
            repo.Seed(SeedProduct("p-1", "seller-2"));
            var current = User("admin-1", role);
            var handler = new UpdateProductCommandHandler(repo, current);

            var result = await handler.Handle(new UpdateProductCommand
            {
                ProductId = "p-1",
                Name = "Updated",
                BrandId = 2,
                CategoryId = 2,
                CostPrice = 99m,
                SoldPrice = 199m,
                Description = "updated"
            }, CancellationToken.None);

            Assert.Equal(99m, result.CostPrice);
            Assert.Equal(199m, result.SoldPrice);
        }

        [Fact]
        public async Task Update_SellerOwner_UpdatesPrices()
        {
            var repo = new ProductRepositoryDouble();
            repo.Seed(SeedProduct("p-1", "seller-1"));
            var current = User("seller-1", "Seller");
            var handler = new UpdateProductCommandHandler(repo, current);

            var result = await handler.Handle(new UpdateProductCommand
            {
                ProductId = "p-1",
                Name = "Updated",
                BrandId = 2,
                CategoryId = 2,
                CostPrice = 99m,
                SoldPrice = 199m,
                Description = "updated"
            }, CancellationToken.None);

            Assert.Equal(99m, result.CostPrice);
        }

        [Fact]
        public async Task Update_SellerNonOwner_ThrowsUnauthorized_AndDoesNotMutate()
        {
            var repo = new ProductRepositoryDouble();
            repo.Seed(SeedProduct("p-1", "seller-2"));
            var current = User("seller-1", "Seller");
            var handler = new UpdateProductCommandHandler(repo, current);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                handler.Handle(new UpdateProductCommand
                {
                    ProductId = "p-1",
                    Name = "Updated",
                    BrandId = 2,
                    CategoryId = 2,
                    CostPrice = 99m,
                    SoldPrice = 199m,
                    Description = "updated"
                }, CancellationToken.None));

            var stored = repo.Get("p-1")!;
            Assert.Equal(10m, stored.CostPrice);
            Assert.Equal(20m, stored.SoldPrice);
            Assert.Equal("Original", stored.Name);
        }

        [Fact]
        public async Task Update_Customer_ThrowsUnauthorized()
        {
            var repo = new ProductRepositoryDouble();
            repo.Seed(SeedProduct("p-1", "seller-1"));
            var current = User("cust-1", "Customer");
            var handler = new UpdateProductCommandHandler(repo, current);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                handler.Handle(new UpdateProductCommand
                {
                    ProductId = "p-1",
                    Name = "Updated",
                    BrandId = 2,
                    CategoryId = 2,
                    CostPrice = 99m,
                    SoldPrice = 199m,
                    Description = "updated"
                }, CancellationToken.None));
        }

        [Fact]
        public async Task Update_Unauthenticated_ThrowsUnauthorized()
        {
            var repo = new ProductRepositoryDouble();
            repo.Seed(SeedProduct("p-1", "seller-1"));
            var current = new FakeCurrentUserService { UserId = null };
            var handler = new UpdateProductCommandHandler(repo, current);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                handler.Handle(new UpdateProductCommand
                {
                    ProductId = "p-1",
                    Name = "Updated",
                    BrandId = 2,
                    CategoryId = 2,
                    CostPrice = 99m,
                    SoldPrice = 199m,
                    Description = "updated"
                }, CancellationToken.None));
        }

        [Fact]
        public async Task Update_Missing_ThrowsNotFound()
        {
            var repo = new ProductRepositoryDouble();
            var current = User("admin-1", "Admin");
            var handler = new UpdateProductCommandHandler(repo, current);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                handler.Handle(new UpdateProductCommand
                {
                    ProductId = "missing",
                    Name = "Updated",
                    BrandId = 2,
                    CategoryId = 2,
                    CostPrice = 99m,
                    SoldPrice = 199m,
                    Description = "updated"
                }, CancellationToken.None));
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("Manager")]
        public async Task Delete_AllowedUnrestrictedRole_Deletes(string role)
        {
            var repo = new ProductRepositoryDouble();
            repo.Seed(SeedProduct("p-1", "seller-2"));
            var current = User("admin-1", role);
            var handler = new DeleteProductCommandHandler(repo, current);

            var result = await handler.Handle(new DeleteProductCommand("p-1"), CancellationToken.None);

            Assert.True(result);
            Assert.Null(repo.Get("p-1"));
        }

        [Fact]
        public async Task Delete_SellerOwner_Deletes()
        {
            var repo = new ProductRepositoryDouble();
            repo.Seed(SeedProduct("p-1", "seller-1"));
            var current = User("seller-1", "Seller");
            var handler = new DeleteProductCommandHandler(repo, current);

            var result = await handler.Handle(new DeleteProductCommand("p-1"), CancellationToken.None);

            Assert.True(result);
            Assert.Null(repo.Get("p-1"));
        }

        [Fact]
        public async Task Delete_SellerNonOwner_ThrowsUnauthorized_AndDoesNotMutate()
        {
            var repo = new ProductRepositoryDouble();
            repo.Seed(SeedProduct("p-1", "seller-2"));
            var current = User("seller-1", "Seller");
            var handler = new DeleteProductCommandHandler(repo, current);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                handler.Handle(new DeleteProductCommand("p-1"), CancellationToken.None));

            Assert.NotNull(repo.Get("p-1"));
        }

        [Fact]
        public async Task Delete_Customer_ThrowsUnauthorized()
        {
            var repo = new ProductRepositoryDouble();
            repo.Seed(SeedProduct("p-1", "seller-1"));
            var current = User("cust-1", "Customer");
            var handler = new DeleteProductCommandHandler(repo, current);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                handler.Handle(new DeleteProductCommand("p-1"), CancellationToken.None));
        }

        [Fact]
        public async Task Delete_Unauthenticated_ThrowsUnauthorized()
        {
            var repo = new ProductRepositoryDouble();
            repo.Seed(SeedProduct("p-1", "seller-1"));
            var current = new FakeCurrentUserService { UserId = null };
            var handler = new DeleteProductCommandHandler(repo, current);

            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                handler.Handle(new DeleteProductCommand("p-1"), CancellationToken.None));
        }

        [Fact]
        public async Task Delete_Missing_ThrowsNotFound()
        {
            var repo = new ProductRepositoryDouble();
            var current = User("admin-1", "Admin");
            var handler = new DeleteProductCommandHandler(repo, current);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                handler.Handle(new DeleteProductCommand("missing"), CancellationToken.None));
        }
    }
}