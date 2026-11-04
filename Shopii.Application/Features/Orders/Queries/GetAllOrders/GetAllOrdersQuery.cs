using MediatR;
using Shopii.Application.Common.Models;
using Shopii.Application.Features.Orders.DTOs;

namespace Shopii.Application.Features.Orders.Queries.GetAllOrders
{
    /// <summary>
    /// Retrieves a page of orders across all users. Admin only — enforced by
    /// the "Admin" authorization policy on the controller.
    /// </summary>
    public sealed record GetAllOrdersQuery(
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<PagedResult<OrderDTO>>, IPagedRequest;
}
