using MediatR;

namespace Shopii.Application.Features.Orders.Commands.DeleteOrder
{
    /// <summary>
    /// Archives an order (soft delete — the row and its history are kept).
    /// Reserved for admins.
    /// </summary>
    public sealed record DeleteOrderCommand(string OrderId) : IRequest<bool>;
}
