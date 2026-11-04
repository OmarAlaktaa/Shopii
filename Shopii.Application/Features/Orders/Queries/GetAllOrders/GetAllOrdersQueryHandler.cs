using MediatR;
using Shopii.Application.Common.Models;
using Shopii.Application.Features.Orders.DTOs;
using Shopii.Application.Interfaces.Persistence;

namespace Shopii.Application.Features.Orders.Queries.GetAllOrders
{
    public sealed class GetAllOrdersQueryHandler
        : IRequestHandler<GetAllOrdersQuery, PagedResult<OrderDTO>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetAllOrdersQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<PagedResult<OrderDTO>> Handle(
            GetAllOrdersQuery request,
            CancellationToken cancellationToken)
        {
            var (orders, totalCount) =
                await _orderRepository.GetPagedAsync(
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
