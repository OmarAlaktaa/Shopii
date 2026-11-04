using MediatR;

namespace Shopii.Application.Features.Products.Commands.DeleteProduct
{
    public sealed record DeleteProductCommand(string ProductId) : IRequest<bool>;
}
