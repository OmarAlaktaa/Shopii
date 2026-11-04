using Shopii.Domain.Entities.Orders;
using Shopii.Domain.Entities.Payments;

namespace Shopii.Application.Interfaces.Persistence
{
    public interface IOrderRepository
    {
        /// <summary>
        /// Loads a single order including its line items (with products) and its
        /// payment (with method and status), plus the order status lookup.
        /// The entity is tracked so callers can mutate it and call UpdateAsync.
        /// Returns null when no order exists with the given id.
        /// </summary>
        Task<Order?> GetByIdAsync(string orderId, CancellationToken cancellationToken);

        Task<(IReadOnlyList<Order> Orders, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<Order> Orders, int TotalCount)> GetPagedByUserIdAsync(
            string userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        /// <summary>
        /// Persists a freshly placed order: inserts the order, its line items and
        /// the payment, and clears the user's cart — all in a single transaction
        /// (one SaveChanges).
        /// </summary>
        Task PlaceOrderAsync(
            Order order,
            Payment payment,
            Shopii.Domain.Entities.Carts.Cart cart,
            CancellationToken cancellationToken);

        Task UpdateAsync(Order order, CancellationToken cancellationToken);

        Task<bool> DeleteAsync(string orderId, CancellationToken cancellationToken);

        /// <summary>Looks up an order status by its title (case-insensitive), e.g. "Pending".</summary>
        Task<OrderStatus?> GetStatusByTitleAsync(string title, CancellationToken cancellationToken);

        Task<OrderStatus?> GetStatusByIdAsync(int orderStatusId, CancellationToken cancellationToken);

        Task<IReadOnlyList<OrderStatus>> GetAllStatusesAsync(CancellationToken cancellationToken);

        Task<PaymentStatus?> GetPaymentStatusByTitleAsync(string title, CancellationToken cancellationToken);

        Task<PaymentMethod?> GetPaymentMethodByIdAsync(int paymentMethodId, CancellationToken cancellationToken);
    }
}
