using MediatR;
using Shopii.Application.Common.Models;
using Shopii.Application.Features.Orders.DTOs;

namespace Shopii.Application.Features.Orders.Queries.GetMyOrders
{
    /// <summary>
    /// Retrieves a page of the current user's own orders.
    /// </summary>
    public sealed record GetMyOrdersQuery(
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<PagedResult<OrderDTO>>, IPagedRequest;
}
