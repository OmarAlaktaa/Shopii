using MediatR;
using Shopii.Application.Common.Models;
using Shopii.Application.Features.Orders.DTOs;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Application.Interfaces.Services;

namespace Shopii.Application.Features.Orders.Queries.GetMyOrders
{
    public sealed class GetMyOrdersQueryHandler
        : IRequestHandler<GetMyOrdersQuery, PagedResult<OrderDTO>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetMyOrdersQueryHandler(
            IOrderRepository orderRepository,
            ICurrentUserService currentUserService)
        {
            _orderRepository = orderRepository;
            _currentUserService = currentUserService;
        }

        public async Task<PagedResult<OrderDTO>> Handle(
            GetMyOrdersQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("A logged-in user is required.");

            var (orders, totalCount) =
                await _orderRepository.GetPagedByUserIdAsync(
                    userId,
                    request.PageNumber,
                    request.PageSize,
                    cancellationToken);

            var orderDTOs = orders
                .Select(order => order.ToDTO())
                .ToList();

            return new PagedResult<OrderDTO>(
                Items: orderDTOs,
                PageNumber: request.PageNumber,
                PageSize: request.PageSize,
                TotalCount: totalCount);
        }
    }
}
