using MediatR;
using Shopii.Application.Features.Orders.DTOs;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Application.Interfaces.Services;
using Shopii.Domain.Entities.Orders;
using Shopii.Domain.Entities.Payments;

namespace Shopii.Application.Features.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandHandler
        : IRequestHandler<CreateOrderCommand, OrderDTO>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly ICurrentUserService _currentUserService;

        public CreateOrderCommandHandler(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            ICurrentUserService currentUserService)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _currentUserService = currentUserService;
        }

        public async Task<OrderDTO> Handle(
            CreateOrderCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("A logged-in user is required.");

            var paymentMethod = await _orderRepository.GetPaymentMethodByIdAsync(
                request.PaymentMethodId,
                cancellationToken);

            if (paymentMethod is null)
            {
                throw new ValidationException(
                    $"Payment method with id '{request.PaymentMethodId}' does not exist.");
            }

            var cart = await _cartRepository.GetByUserIdAsync(
                userId,
                cancellationToken);

            if (cart is null || cart.Items.Count == 0)
            {
                throw new ValidationException("Your cart is empty; add products before placing an order.");
            }

            // A cart item without a product means the product row is gone or
            // soft-deleted — it cannot be ordered.
            var unavailableProductIds = cart.Items
                .Where(item => item.Product is null)
                .Select(item => item.ProductId)
                .ToList();

            if (unavailableProductIds.Count > 0)
            {
                throw new ValidationException(
                    "These cart products are no longer available: "
                    + string.Join(", ", unavailableProductIds));
            }

            // Missing lookup rows are a server-side data/configuration fault,
            // not something the client can fix, so they stay a 500 rather than
            // a validation error.
            var pendingOrderStatus = await _orderRepository.GetStatusByTitleAsync(
                "Pending",
                cancellationToken)
                ?? throw new InvalidOperationException(
                    "Order status lookup row 'Pending' is missing from the database.");

            var pendingPaymentStatus = await _orderRepository.GetPaymentStatusByTitleAsync(
                "Pending",
                cancellationToken)
                ?? throw new InvalidOperationException(
                    "Payment status lookup row 'Pending' is missing from the database.");

            // Unit prices are snapshotted from the current product price.
            var orderItems = cart.Items
                .Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.SoldPrice
                })
                .ToList();

            var totalAmount = orderItems.Sum(item => item.UnitPrice * item.Quantity);

            var payment = new Payment
            {
                Amount = totalAmount,
                PaymentMethodId = paymentMethod.PaymentMethodId,
                PaymentStatusId = pendingPaymentStatus.PaymentStatusId,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId
            };

            var order = new Order
            {
                OrderId = Guid.NewGuid().ToString(),
                UserId = userId,
                OrderStatusId = pendingOrderStatus.OrderStatusId,
                CreatedAt = DateTime.UtcNow,
                OrderItems = orderItems,
                // Linking the navigation makes the insert dependency explicit:
                // EF then generates the payment id first and stamps it onto
                // the order instead of inserting the order with a dangling
                // PaymentId.
                Payment = payment
            };

            // One SaveChanges: the order, its items, the payment and the cart
            // clearing all succeed or fail together.
            await _orderRepository.PlaceOrderAsync(
                order,
                payment,
                cart,
                cancellationToken);

            // Reload the lookups so the returned DTO carries their titles.
            order.Status = pendingOrderStatus;
            payment.PaymentMethod = paymentMethod;
            payment.Status = pendingPaymentStatus;

            return order.ToDTO();
        }
    }
}
