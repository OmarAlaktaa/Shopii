using MediatR;
using Shopii.Application.Features.Carts.DTOs;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Application.Interfaces.Services;

namespace Shopii.Application.Features.Carts.Commands.RemoveCartItem
{
    public class RemoveCartItemCommandHandler
        : IRequestHandler<RemoveCartItemCommand, CartDTO>
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICurrentUserService _currentUserService;

        public RemoveCartItemCommandHandler(
            ICartRepository cartRepository,
            ICurrentUserService currentUserService)
        {
            _cartRepository = cartRepository;
            _currentUserService = currentUserService;
        }

        public async Task<CartDTO> Handle(
            RemoveCartItemCommand request,
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

            cart.Items.Remove(item);

            await _cartRepository.UpdateAsync(cart, cancellationToken);

            return cart.ToDTO();
        }
    }
}
