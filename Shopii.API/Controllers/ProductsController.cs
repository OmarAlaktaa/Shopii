using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopii.API.Models;
using Shopii.Application.Common.Auditing;
using Shopii.Application.Features.Products.Commands.CreateProduct;
using Shopii.Application.Features.Products.Commands.DeleteProduct;
using Shopii.Application.Features.Products.Commands.UpdateProduct;
using Shopii.Application.Features.Products.Queries.GetAllProducts;
using Shopii.Application.Features.Products.Queries.GetProductById;
using Shopii.Application.Interfaces.Services;

namespace Shopii.API.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        // MediatR dispatches commands and queries to their corresponding handlers.
        private readonly IMediator _mediator;
        private readonly IAuditLogger _auditLogger;

        public ProductsController(
            IMediator mediator,
            IAuditLogger auditLogger)
        {
            _mediator = mediator;
            _auditLogger = auditLogger;
        }

        // GET: api/products?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetAllProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetAllProductsQuery(pageNumber, pageSize);
            var products = await _mediator.Send(query, cancellationToken);
            return Ok(products);
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(
            string id,
            CancellationToken cancellationToken)
        {
            var product = await _mediator.Send(
                new GetProductByIdQuery(id),
                cancellationToken);

            if (product is null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // POST: api/products
        [Authorize(Roles = "Admin,Manager,Seller")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct(
            [FromBody] CreateProductCommand command,
            CancellationToken cancellationToken)
        {
            var product = await _mediator.Send(command, cancellationToken);

            await _auditLogger.LogAsync(
                new AuditEvent(
                    AuditAction.ProductCreated,
                    AuditEntityTypes.Product,
                    product.ProductId,
                    Details: AuditDetails.Create(
                        ("BrandId", product.BrandId),
                        ("CategoryId", product.CategoryId),
                        ("SoldPrice", product.SoldPrice))),
                cancellationToken);

            return CreatedAtAction(
                nameof(GetProductById),
                new { id = product.ProductId },
                product);
        }

        // PUT: api/products/{id}
        [Authorize(Roles = "Admin,Manager,Seller")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(
            string id,
            [FromBody] UpdateProductRequest request,
            CancellationToken cancellationToken)
        {
            // The route is the single source of truth for the product id;
            // the body only carries the editable fields.
            var command = new UpdateProductCommand
            {
                ProductId = id,
                Name = request.Name,
                BrandId = request.BrandId,
                CategoryId = request.CategoryId,
                CostPrice = request.CostPrice,
                SoldPrice = request.SoldPrice,
                Description = request.Description,
            };

            var product = await _mediator.Send(command, cancellationToken);

            await _auditLogger.LogAsync(
                new AuditEvent(
                    AuditAction.ProductUpdated,
                    AuditEntityTypes.Product,
                    id,
                    Details: AuditDetails.Create(
                        ("BrandId", product.BrandId),
                        ("CategoryId", product.CategoryId),
                        ("SoldPrice", product.SoldPrice))),
                cancellationToken);

            return Ok(product);
        }

        // DELETE: api/products/{id}
        [Authorize(Roles = "Admin,Manager,Seller")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(
            string id,
            CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteProductCommand(id), cancellationToken);

            await _auditLogger.LogAsync(
                new AuditEvent(
                    AuditAction.ProductDeleted,
                    AuditEntityTypes.Product,
                    id),
                cancellationToken);

            return NoContent();
        }
    }
}
