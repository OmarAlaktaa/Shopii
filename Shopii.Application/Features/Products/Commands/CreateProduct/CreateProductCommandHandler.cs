using MediatR;
using Shopii.Application.Features.Products.DTOs;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Application.Interfaces.Services;
using Shopii.Domain.Entities.Products;

namespace Shopii.Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler
        : IRequestHandler<CreateProductCommand, ProductDTO>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUserService _currentUserService;

        public CreateProductCommandHandler(IProductRepository productRepository)
            : this(productRepository, new NullCurrentUserService())
        {
        }

        public CreateProductCommandHandler(
            IProductRepository productRepository,
            ICurrentUserService currentUserService)
        {
            _productRepository = productRepository;
            _currentUserService = currentUserService;
        }

        public CreateProductCommandHandler(
            ICurrentUserService currentUserService,
            IProductRepository productRepository)
            : this(productRepository, currentUserService)
        {
        }

        public async Task<ProductDTO> Handle(
            CreateProductCommand request,
            CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId
                ?? throw new UnauthorizedException(
                    "A logged-in user is required.");

            if (!IsAllowedRole(_currentUserService.Role))
            {
                throw new UnauthorizedException(
                    "Only admins, managers, and sellers can create products.");
            }

            var product = new Product
            {
                ProductId = Guid.NewGuid().ToString(),
                Name = request.Name,
                BrandId = request.BrandId,
                CategoryId = request.CategoryId,
                CostPrice = request.CostPrice,
                SoldPrice = request.SoldPrice,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUserId
            };

            await _productRepository.AddAsync(product, cancellationToken);

            return product.ToDTO();
        }

        private static bool IsAllowedRole(string? role) =>
            string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "Seller", StringComparison.OrdinalIgnoreCase);

        private sealed class NullCurrentUserService : ICurrentUserService
        {
            public string? UserId => null;
            public string? Role => null;
            public bool IsAdmin => false;
        }
    }
}