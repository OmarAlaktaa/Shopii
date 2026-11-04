using MediatR;
using Shopii.Application.Features.Carts.DTOs;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Application.Interfaces.Services;

namespace Shopii.Application.Features.Carts.Commands.UpdateCartItemQuantity
{
    public class UpdateCartItemQuantityCommandHandler
        : IRequestHandler<UpdateCartItemQuantityCommand, CartDTO>
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICurrentUserService _currentUserService;

        public UpdateCartItemQuantityCommandHandler(
            ICartRepository cartRepository,
            ICurrentUserService currentUserService)
        {
            _cartRepository = cartRepository;
            _currentUserService = currentUserService;
        }

        public async Task<CartDTO> Handle(
            UpdateCartItemQuantityCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("A logged-in user is required.");

            var cart = await _cartRepository.GetByUserIdAsync(
                userId,
                cancellationToken);

            var item = cart?.Items
                .SingleOrDefault(i => i.ProductId == request.ProductId);

            if (cart is null || item is null)
            {
                throw new NotFoundException(
                    $"Product with id '{request.ProductId}' is not in your cart.");
            }

            // Absolute assignment: repeating the same request leaves the cart
            // in the same state, unlike the additive POST endpoint.
            item.Quantity = request.Quantity;

            await _cartRepository.UpdateAsync(cart, cancellationToken);

            return cart.ToDTO();
        }
    }
}
