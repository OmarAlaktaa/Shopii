using MediatR;
using Shopii.Application.Features.Orders.DTOs;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Application.Interfaces.Services;
using Shopii.Domain.Entities.Orders;

namespace Shopii.Application.Features.Orders.Commands.UpdateOrder
{
    public class UpdateOrderCommandHandler
        : IRequestHandler<UpdateOrderCommand, OrderDTO>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUserService _currentUserService;

        public UpdateOrderCommandHandler(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ICurrentUserService currentUserService)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _currentUserService = currentUserService;
        }

        public async Task<OrderDTO> Handle(
            UpdateOrderCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("A logged-in user is required.");

            var order = await _orderRepository.GetByIdAsync(
                request.OrderId,
                cancellationToken);

            if (order is null)
            {
                throw new NotFoundException(
                    $"Order with id '{request.OrderId}' was not found.");
            }

            // The owner can edit their own order; admins can edit any.
            if (order.UserId != userId && !_currentUserService.IsAdmin)
            {
                throw new UnauthorizedException("You can only update your own orders.");
            }

            // Order history is immutable once the order progressed past
            // "Pending" — items and payment can no longer be rewritten.
            if (!string.Equals(order.Status?.Title, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException(
                    $"An order with status '{order.Status?.Title}' can no longer be edited; only 'Pending' orders can.");
            }

            var paymentMethod = await _orderRepository.GetPaymentMethodByIdAsync(
                request.PaymentMethodId,
                cancellationToken);

            if (paymentMethod is null)
            {
                throw new ValidationException(
                    $"Payment method with id '{request.PaymentMethodId}' does not exist.");
            }

            // Every requested product must exist (and not be soft-deleted).
            var distinctProductIds = request.Items
                .Select(item => item.ProductId)
                .Distinct()
                .ToList();

            var missingProductIds = new List<string>();

            foreach (var productId in distinctProductIds)
            {
                var product = await _productRepository.GetByIdAsync(
                    productId,
                    cancellationToken);

                if (product is null)
                {
                    missingProductIds.Add(productId);
                    continue;
                }

                // Snapshot the current price into the replacement line item.
                var orderItem = order.OrderItems
                    .FirstOrDefault(i => i.ProductId == productId);

                if (orderItem is null)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = productId,
                        Quantity = request.Items.First(i => i.ProductId == productId).Quantity,
                        UnitPrice = product.SoldPrice
                    });
                }
                else
                {
                    orderItem.Quantity = request.Items.First(i => i.ProductId == productId).Quantity;
                    orderItem.UnitPrice = product.SoldPrice;
                }
            }

            if (missingProductIds.Count > 0)
            {
                throw new ValidationException(
                    "These products are no longer available: "
                    + string.Join(", ", missingProductIds));
            }

            // Drop line items that are no longer part of the order. They are
            // tracked, so severing them from the collection deletes the rows
            // on SaveChanges.
            var requestedIds = distinctProductIds.ToHashSet();
            var removedItems = order.OrderItems
                .Where(i => !requestedIds.Contains(i.ProductId))
                .ToList();

            foreach (var removedItem in removedItems)
            {
                order.OrderItems.Remove(removedItem);
            }

            var totalAmount = order.OrderItems.Sum(i => i.UnitPrice * i.Quantity);

            order.Payment.Amount = totalAmount;

            // Re-pointing the FK also requires re-pointing the navigation,
            // otherwise EF severs it and the DTO would lose the method title.
            // UpdateAsync pins the lookup entity to Unchanged, so attaching
            // it does not rewrite the reference row.
            //
            // Only re-point when the method actually changes: the order was
            // loaded tracked with its PaymentMethod included, so when the id
            // stays the same, assigning this AsNoTracking instance would make
            // EF track two entities with the same key and throw an identity
            // conflict in DetectChanges.
            if (order.Payment.PaymentMethodId != paymentMethod.PaymentMethodId)
            {
                order.Payment.PaymentMethodId = paymentMethod.PaymentMethodId;
                order.Payment.PaymentMethod = paymentMethod;
            }

            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedByUserId = userId;

            await _orderRepository.UpdateAsync(order, cancellationToken);

            return order.ToDTO();
        }
    }
}
