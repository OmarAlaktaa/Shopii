using MediatR;
using Shopii.Application.Features.Orders.DTOs;

namespace Shopii.Application.Features.Orders.Queries.GetOrderById
{
    /// <summary>
    /// Retrieves a single order by its identifier. Owners can read their own
    /// orders; admins can read any. Anything else returns null (404), which
    /// avoids leaking which order ids exist.
    /// </summary>
    public record GetOrderByIdQuery(string OrderId)
        : IRequest<OrderDTO?>;
}
