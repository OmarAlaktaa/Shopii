using MediatR;
using Shopii.Application.Features.Carts.DTOs;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Application.Interfaces.Services;

namespace Shopii.Application.Features.Carts.Queries.GetMyCart
{
    public sealed class GetMyCartQueryHandler
        : IRequestHandler<GetMyCartQuery, CartDTO>
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetMyCartQueryHandler(
            ICartRepository cartRepository,
            ICurrentUserService currentUserService)
        {
            _cartRepository = cartRepository;
            _currentUserService = currentUserService;
        }

        public async Task<CartDTO> Handle(
            GetMyCartQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("A logged-in user is required.");

            var cart = await _cartRepository.GetByUserIdAsync(
                userId,
                cancellationToken);

            if (cart is null)
            {
                // No cart yet: return an empty one so the client has a single
                // shape to render.
                return new CartDTO { UserId = userId };
            }

            return cart.ToDTO();
        }
    }
}
