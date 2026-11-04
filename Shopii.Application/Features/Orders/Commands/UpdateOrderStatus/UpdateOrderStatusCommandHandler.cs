using MediatR;
using Shopii.Application.Features.Orders.DTOs;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Application.Interfaces.Services;

namespace Shopii.Application.Features.Orders.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusCommandHandler
        : IRequestHandler<UpdateOrderStatusCommand, OrderDTO>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentUserService _currentUserService;

        public UpdateOrderStatusCommandHandler(
            IOrderRepository orderRepository,
            ICurrentUserService currentUserService)
        {
            _orderRepository = orderRepository;
            _currentUserService = currentUserService;
        }

        public async Task<OrderDTO> Handle(
            UpdateOrderStatusCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("A logged-in user is required.");

            // Defense in depth: the controller already requires the "Admin"
            // policy, but the rule is enforced here too.
            if (!_currentUserService.IsAdmin)
            {
                throw new UnauthorizedException("Only admins can change the status of an order.");
            }

            var order = await _orderRepository.GetByIdAsync(
                request.OrderId,
                cancellationToken);

            if (order is null)
            {
                throw new NotFoundException(
                    $"Order with id '{request.OrderId}' was not found.");
            }

            var newStatus = await _orderRepository.GetStatusByIdAsync(
                request.OrderStatusId,
                cancellationToken);

            if (newStatus is null)
            {
                throw new ValidationException(
                    $"Order status with id '{request.OrderStatusId}' does not exist.");
            }

            order.OrderStatusId = newStatus.OrderStatusId;
            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedByUserId = userId;

            await _orderRepository.UpdateAsync(order, cancellationToken);

            return order.ToDTO();
        }
    }
}
