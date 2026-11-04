using Microsoft.EntityFrameworkCore;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Domain.Entities.Carts;
using Shopii.Domain.Entities.Orders;
using Shopii.Domain.Entities.Payments;

namespace Shopii.Infrastructure.Persistence.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ShopiiDbContext _context;

        public OrderRepository(ShopiiDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetByIdAsync(string orderId, CancellationToken cancellationToken)
        {
            // Tracked (no AsNoTracking): the update flow mutates the loaded order,
            // its items and its payment, then persists through UpdateAsync.
            return await ApplyIncludes(_context.Orders)
                .SingleOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
        }

        public async Task<(IReadOnlyList<Order> Orders, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = ApplyIncludes(_context.Orders).AsNoTracking();

            var totalCount = await query.CountAsync(cancellationToken);

            var orders = await query
                .OrderByDescending(order => order.CreatedAt)
                .ThenBy(order => order.OrderId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (orders, totalCount);
        }

        public async Task<(IReadOnlyList<Order> Orders, int TotalCount)> GetPagedByUserIdAsync(
            string userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = ApplyIncludes(_context.Orders)
                .AsNoTracking()
                .Where(order => order.UserId == userId);

            var totalCount = await query.CountAsync(cancellationToken);

            var orders = await query
                .OrderByDescending(order => order.CreatedAt)
                .ThenBy(order => order.OrderId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (orders, totalCount);
        }

        public async Task PlaceOrderAsync(
            Order order,
            Payment payment,
            Cart cart,
            CancellationToken cancellationToken)
        {
            await _context.Orders.AddAsync(order, cancellationToken);
            await _context.Payments.AddAsync(payment, cancellationToken);

            // Emptying the cart is part of the same SaveChanges, so the order is
            // only placed (and the cart only cleared) atomically — a failure
            // leaves both the order and the cart untouched.
            cart.Items.Clear();

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Order order, CancellationToken cancellationToken)
        {
            _context.Orders.Update(order);

            // Update() attaches untracked entities reachable through
            // navigations (order status, payment method/status) as Modified,
            // which would rewrite lookup/reference rows with their own
            // values. They are never edited by this API, so pin them back
            // to Unchanged.
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                if (entry.Entity is OrderStatus or PaymentMethod or PaymentStatus)
                {
                    entry.State = EntityState.Unchanged;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> DeleteAsync(string orderId, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .SingleOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);

            if (order is null)
            {
                return false;
            }

            // Remove() is rewritten into a soft delete by the SoftDeleteInterceptor
            // (Order inherits ISoftDeletable from BaseEntity), so the row and its
            // history are kept and only stamped with who/when it was archived.
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<OrderStatus?> GetStatusByTitleAsync(string title, CancellationToken cancellationToken)
        {
            // ToLower() keeps the comparison case-insensitive while still being
            // translatable to SQL.
            var normalized = title.ToLower();

            return await _context.OrderStatuses
                .AsNoTracking()
                .SingleOrDefaultAsync(s => s.Title.ToLower() == normalized, cancellationToken);
        }

        public async Task<OrderStatus?> GetStatusByIdAsync(int orderStatusId, CancellationToken cancellationToken)
        {
            return await _context.OrderStatuses
                .AsNoTracking()
                .SingleOrDefaultAsync(s => s.OrderStatusId == orderStatusId, cancellationToken);
        }

        public async Task<IReadOnlyList<OrderStatus>> GetAllStatusesAsync(CancellationToken cancellationToken)
        {
            return await _context.OrderStatuses
                .AsNoTracking()
                .OrderBy(s => s.OrderStatusId)
                .ToListAsync(cancellationToken);
        }

        public async Task<PaymentStatus?> GetPaymentStatusByTitleAsync(string title, CancellationToken cancellationToken)
        {
            var normalized = title.ToLower();

            return await _context.PaymentStatuses
                .AsNoTracking()
                .SingleOrDefaultAsync(s => s.Title.ToLower() == normalized, cancellationToken);
        }

        public async Task<PaymentMethod?> GetPaymentMethodByIdAsync(int paymentMethodId, CancellationToken cancellationToken)
        {
            return await _context.PaymentMethods
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.PaymentMethodId == paymentMethodId, cancellationToken);
        }

        // Everything a DTO needs so mapping never triggers lazy loads or null
        // refs: line items with products, payment with method and status, and
        // the order status lookup row.
        private static IQueryable<Order> ApplyIncludes(IQueryable<Order> query)
        {
            return query
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Payment)
                    .ThenInclude(p => p.PaymentMethod)
                .Include(o => o.Payment)
                    .ThenInclude(p => p.Status)
                .Include(o => o.Status);
        }
    }
}
