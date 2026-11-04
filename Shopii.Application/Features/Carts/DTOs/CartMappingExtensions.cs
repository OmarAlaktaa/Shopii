using Shopii.Domain.Entities.Carts;

namespace Shopii.Application.Features.Carts.DTOs
{
    public static class CartMappingExtensions
    {
        public static CartDTO ToDTO(this Cart cart)
        {
            var items = cart.Items
                // A cart item whose product row is missing (or soft-deleted)
                // cannot be priced or named; skip it instead of failing the
                // whole cart read.
                .Where(item => item.Product is not null)
                .Select(item => new CartItemDTO
                {
                    CartItem = item.Item,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.SoldPrice,
                    LineTotal = item.Product.SoldPrice * item.Quantity
                })
                .ToList();

            return new CartDTO
            {
                CartId = cart.CartId,
                UserId = cart.UserId,
                Items = items,
                TotalAmount = items.Sum(item => item.LineTotal)
            };
        }
    }
}
