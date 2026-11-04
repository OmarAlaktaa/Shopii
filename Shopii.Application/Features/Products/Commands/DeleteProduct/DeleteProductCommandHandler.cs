using MediatR;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Application.Interfaces.Services;
using Shopii.Domain.Entities.Products;

namespace Shopii.Application.Features.Products.Commands.DeleteProduct
{
    public class DeleteProductCommandHandler
        : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUserService _currentUserService;

        public DeleteProductCommandHandler(IProductRepository productRepository)
            : this(productRepository, new NullCurrentUserService())
        {
        }

        public DeleteProductCommandHandler(
            IProductRepository productRepository,
            ICurrentUserService currentUserService)
        {
            _productRepository = productRepository;
            _currentUserService = currentUserService;
        }

        public DeleteProductCommandHandler(
            ICurrentUserService currentUserService,
            IProductRepository productRepository)
            : this(productRepository, currentUserService)
        {
        }

        public async Task<bool> Handle(
            DeleteProductCommand request,
            CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId
                ?? throw new UnauthorizedException(
                    "A logged-in user is required.");

            if (!IsAllowedRole(_currentUserService.Role))
            {
                throw new UnauthorizedException(
                    "Only admins, managers, and sellers can delete products.");
            }

            // Enforce ownership for sellers before mutating. A missing product
            // is handled by DeleteAsync below to preserve idempotent soft-delete.
            var product = await _productRepository.GetByIdAsync(
                request.ProductId,
                cancellationToken);

            if (product is not null &&
                IsSeller(_currentUserService.Role) &&
                !string.Equals(
                    product.CreatedByUserId,
                    currentUserId,
                    StringComparison.Ordinal))
            {
                throw new UnauthorizedException(
                    "You are not allowed to delete this product.");
            }

            var deleted = await _productRepository.DeleteAsync(
                request.ProductId,
                cancellationToken);

            if (!deleted)
            {
                throw new NotFoundException(
                    $"Product with id '{request.ProductId}' was not found.");
            }

            return true;
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