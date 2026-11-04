using MediatR;
using Shopii.Application.Features.Carts.DTOs;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Application.Interfaces.Services;
using Shopii.Domain.Entities.Carts;

namespace Shopii.Application.Features.Carts.Commands.AddCartItem
{
    public class AddCartItemCommandHandler
        : IRequestHandler<AddCartItemCommand, CartDTO>
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUserService _currentUserService;

        public AddCartItemCommandHandler(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            ICurrentUserService currentUserService)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _currentUserService = currentUserService;
        }

        public async Task<CartDTO> Handle(
            AddCartItemCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("A logged-in user is required.");

            // Refuse products that do not exist (or are soft-deleted) before
            // touching the cart.
            var product = await _productRepository.GetByIdAsync(
                request.ProductId,
                cancellationToken);

            if (product is null)
            {
                throw new NotFoundException(
                    $"Product with id '{request.ProductId}' was not found.");
            }

            var cart = await _cartRepository.GetByUserIdAsync(
                userId,
                cancellationToken);

            if (cart is null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
                cart.Items.Add(new CartItem
                {
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                });

                await _cartRepository.AddAsync(cart, cancellationToken);

                // Re-read so the response includes the product details of
                // the new line (the item was created without its Product
                // navigation loaded, so ToDTO would drop it).
                var refreshedNew = await _cartRepository.GetByUserIdAsync(
                    userId, cancellationToken);

                return refreshedNew?.ToDTO() ?? cart.ToDTO();
            }

            var existingItem = cart.Items
                .SingleOrDefault(item => item.ProductId == request.ProductId);

            if (existingItem is not null)
            {
                // Adding the same product again just increases the quantity.
                existingItem.Quantity += request.Quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                });
            }

            await _cartRepository.UpdateAsync(cart, cancellationToken);

            // Re-read so the response includes the product details of any
            // just-added line (its Product navigation is not loaded yet, so
            // ToDTO would drop it from the response).
            var refreshed = await _cartRepository.GetByUserIdAsync(
                userId, cancellationToken);

            return refreshed?.ToDTO() ?? cart.ToDTO();
        }
    }
}
