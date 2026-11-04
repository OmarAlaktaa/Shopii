using MediatR;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Application.Interfaces.Services;

namespace Shopii.Application.Features.Orders.Commands.DeleteOrder
{
    public class DeleteOrderCommandHandler
        : IRequestHandler<DeleteOrderCommand, bool>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentUserService _currentUserService;

        public DeleteOrderCommandHandler(
            IOrderRepository orderRepository,
            ICurrentUserService currentUserService)
        {
            _orderRepository = orderRepository;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(
            DeleteOrderCommand request,
            CancellationToken cancellationToken)
        {
            // Defense in depth: the controller already requires the "Admin"
            // policy, but the rule is enforced here too.
            if (!_currentUserService.IsAdmin)
            {
                throw new UnauthorizedException("Only admins can delete orders.");
            }

            var deleted = await _orderRepository.DeleteAsync(
                request.OrderId,
                cancellationToken);

            if (!deleted)
            {
                throw new NotFoundException(
                    $"Order with id '{request.OrderId}' was not found.");
            }

            return true;
        }
    }
}
