using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopii.API.Models;
using Shopii.Application.Features.Orders.Commands.CreateOrder;
using Shopii.Application.Features.Orders.Commands.DeleteOrder;
using Shopii.Application.Features.Orders.Commands.UpdateOrder;
using Shopii.Application.Features.Orders.DTOs;
using Shopii.Application.Features.Orders.Queries.GetMyOrders;
using Shopii.Application.Features.Orders.Queries.GetOrderById;

namespace Shopii.API.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        // MediatR dispatches commands and queries to their corresponding handlers.
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/orders?pageNumber=1&pageSize=10
        // The current user's own orders.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMyOrders(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var orders = await _mediator.Send(
                new GetMyOrdersQuery(pageNumber, pageSize),
                cancellationToken);

            return Ok(orders);
        }

        // GET: api/orders/{id}
        // Owners can read their own orders; admins can read any.
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(
            string id,
            CancellationToken cancellationToken)
        {
            var order = await _mediator.Send(
                new GetOrderByIdQuery(id),
                cancellationToken);

            if (order is null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        // POST: api/orders
        // Places an order from the current user's cart.
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateOrder(
            [FromBody] CreateOrderCommand command,
            CancellationToken cancellationToken)
        {
            var order = await _mediator.Send(command, cancellationToken);

            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = order.OrderId },
                order);
        }

        // PUT: api/orders/{id}
        // Replaces items and payment method; only allowed while Pending.
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(
            string id,
            [FromBody] UpdateOrderRequest request,
            CancellationToken cancellationToken)
        {
            // The route is the single source of truth for the order id;
            // the body only carries the editable fields.
            var command = new UpdateOrderCommand
            {
                OrderId = id,
                PaymentMethodId = request.PaymentMethodId,
                Items = request.Items
                    .Select(item => new UpdateOrderItemDTO
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    })
                    .ToList()
            };

            var order = await _mediator.Send(command, cancellationToken);
            return Ok(order);
        }

        // DELETE: api/orders/{id}
        // Archives the order (soft delete).
        [Authorize(Policy = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(
            string id,
            CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteOrderCommand(id), cancellationToken);
            return NoContent();
        }
    }
}
