using MediatR;
using Shopii.Application.Features.Products.DTOs;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Application.Interfaces.Services;

namespace Shopii.Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler
        : IRequestHandler<UpdateProductCommand, ProductDTO>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUserService _currentUserService;

        public UpdateProductCommandHandler(IProductRepository productRepository)
            : this(productRepository, new NullCurrentUserService())
        {
        }

        public UpdateProductCommandHandler(
            IProductRepository productRepository,
            ICurrentUserService currentUserService)
        {
            _productRepository = productRepository;
            _currentUserService = currentUserService;
        }

        public UpdateProductCommandHandler(
            ICurrentUserService currentUserService,
            IProductRepository productRepository)
            : this(productRepository, currentUserService)
        {
        }

        public async Task<ProductDTO> Handle(
            UpdateProductCommand request,
            CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId
                ?? throw new UnauthorizedException(
                    "A logged-in user is required.");

            if (!IsAllowedRole(_currentUserService.Role))
            {
                throw new UnauthorizedException(
                    "Only admins, managers, and sellers can update products.");
            }

            var product = await _productRepository.GetByIdAsync(
                request.ProductId,
                cancellationToken);

            if (product is null)
            {
                throw new NotFoundException(
                    $"Product with id '{request.ProductId}' was not found.");
            }

            // Sellers may only manage products they own. Admins and managers
            // are not restricted by ownership.
            if (IsSeller(_currentUserService.Role) &&
                !string.Equals(
                    product.CreatedByUserId,
                    currentUserId,
                    StringComparison.Ordinal))
            {
                throw new UnauthorizedException(
                    "You are not allowed to update this product.");
            }

            product.Name = request.Name;
            product.BrandId = request.BrandId;
            product.CategoryId = request.CategoryId;
            product.CostPrice = request.CostPrice;
            product.SoldPrice = request.SoldPrice;
            product.Description = request.Description;
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedByUserId = currentUserId;

            await _productRepository.UpdateAsync(product, cancellationToken);

            return product.ToDTO();
        }

        private static bool IsAllowedRole(string? role) =>
            string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "Seller", StringComparison.OrdinalIgnoreCase);

        private static bool IsSeller(string? role) =>
            string.Equals(role, "Seller", StringComparison.OrdinalIgnoreCase);

        private sealed class NullCurrentUserService : ICurrentUserService
        {
            public string? UserId => null;
            public string? Role => null;
            public bool IsAdmin => false;
        }
    }
}