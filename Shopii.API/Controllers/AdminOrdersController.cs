using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopii.Application.Common.Auditing;
using Shopii.Application.Features.Orders.Commands.UpdateOrderStatus;
using Shopii.Application.Features.Orders.Queries.GetAllOrders;
using Shopii.API.Models;
using Shopii.Application.Interfaces.Services;

namespace Shopii.API.Controllers
{
    // Admin-only order management endpoints.
    [Authorize(Policy = "Admin")]
    [Route("api/admin/orders")]
    [ApiController]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IAuditLogger _auditLogger;

        public AdminOrdersController(
            IMediator mediator,
            IAuditLogger auditLogger)
        {
            _mediator = mediator;
            _auditLogger = auditLogger;
        }

        // GET: api/admin/orders?pageNumber=1&pageSize=10
        // Every user's orders, newest first.
        [HttpGet]
        public async Task<IActionResult> GetAllOrders(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var orders = await _mediator.Send(
                new GetAllOrdersQuery(pageNumber, pageSize),
                cancellationToken);

            await _auditLogger.LogAsync(
                new AuditEvent(
                    AuditAction.OrdersListViewed,
                    AuditEntityTypes.Order,
                    Details: AuditDetails.Create(
                        ("PageNumber", orders.PageNumber),
                        ("PageSize", orders.PageSize),
                        ("TotalCount", orders.TotalCount))),
                cancellationToken);

            return Ok(orders);
        }

        // PUT: api/admin/orders/{id}/status
        // Moves an order to a different status (e.g. Paid, Shipped, Delivered).
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(
            string id,
            [FromBody] UpdateOrderStatusRequest request,
            CancellationToken cancellationToken)
        {
            var order = await _mediator.Send(
                new UpdateOrderStatusCommand
                {
                    OrderId = id,
                    OrderStatusId = request.OrderStatusId
                },
                cancellationToken);

            await _auditLogger.LogAsync(
                new AuditEvent(
                    AuditAction.OrderStatusChanged,
                    AuditEntityTypes.Order,
                    id,
                    Details: AuditDetails.Create(
                        ("OrderStatusId", request.OrderStatusId))),
                cancellationToken);

            return Ok(order);
        }
    }
}
