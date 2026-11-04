using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopii.API.Models;
using Shopii.Application.Features.Carts.Commands.AddCartItem;
using Shopii.Application.Features.Carts.Commands.RemoveCartItem;
using Shopii.Application.Features.Carts.Commands.UpdateCartItemQuantity;
using Shopii.Application.Features.Carts.Queries.GetMyCart;

namespace Shopii.API.Controllers
{
    [Route("api/carts")]
    [ApiController]
    [Authorize]
    public class CartsController : ControllerBase
    {
        // MediatR dispatches commands and queries to their corresponding handlers.
        private readonly IMediator _mediator;

        public CartsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/carts
        // The current user's cart (empty when none exists yet).
        [HttpGet]
        public async Task<IActionResult> GetMyCart(
            CancellationToken cancellationToken)
        {
            var cart = await _mediator.Send(new GetMyCartQuery(), cancellationToken);
            return Ok(cart);
        }

        // POST: api/carts
        // Adds a product; adding the same product again increases its quantity.
        [HttpPost]
        public async Task<IActionResult> AddCartItem(
            [FromBody] AddCartItemCommand command,
            CancellationToken cancellationToken)
        {
            var cart = await _mediator.Send(command, cancellationToken);
            return Ok(cart);
        }

        // PUT: api/carts/{productId}
        // Sets the quantity of a product already in the cart (absolute value,
        // so repeating the same request is safe).
        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateCartItemQuantity(
            string productId,
            [FromBody] UpdateCartItemRequest request,
            CancellationToken cancellationToken)
        {
            // The route is the single source of truth for the product id;
            // the body only carries the editable field.
            var cart = await _mediator.Send(
                new UpdateCartItemQuantityCommand(productId, request.Quantity),
                cancellationToken);

            return Ok(cart);
        }

        // DELETE: api/carts/{productId}
        // Removes one product from the cart.
        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveCartItem(
            string productId,
            CancellationToken cancellationToken)
        {
            var cart = await _mediator.Send(
                new RemoveCartItemCommand(productId),
                cancellationToken);

            return Ok(cart);
        }
    }
}
