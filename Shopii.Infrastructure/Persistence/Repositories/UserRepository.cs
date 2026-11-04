using Microsoft.EntityFrameworkCore;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Domain.Entities.Users;

namespace Shopii.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        public readonly ShopiiDbContext _context;
        public UserRepository(ShopiiDbContext context)
        {
            _context = context;
        }
        
        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        {
            var normalizedEmail = NormalizeEmail(email);

            // The role is included because the login flow embeds the role
            // title into the JWT. Collate(...) forces NOCASE even if the query
            // is run against a database whose column predates the collation
            // change.
            return await _context.Users
                .Include(u => u.Role)
                .SingleOrDefaultAsync(
                    u => EF.Functions.Collate(u.Email, "NOCASE") == normalizedEmail,
                    cancellationToken);
        }

        public Task<bool> AnyActiveByEmailAsync(
            string email,
            CancellationToken cancellationToken)
        {
            var normalizedEmail = NormalizeEmail(email);

            // The predicate is written out explicitly (instead of relying only
            // on the global query filter) so the registration rule is clear:
            // an active account owns the address, a soft-deleted one does not.
            // IgnoreQueryFilters also makes this independent of future changes
            // to the filter configuration.
            return _context.Users
                .IgnoreQueryFilters()
                .AnyAsync(
                    u => EF.Functions.Collate(u.Email, "NOCASE") == normalizedEmail
                        && u.DeletedAt == null,
                    cancellationToken);
        }

        public async Task<User?> GetByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken)
        {
            user.Email = NormalizeEmail(user.Email);

            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task UpdateAsync(User user, CancellationToken cancellationToken)
        {
            user.Email = NormalizeEmail(user.Email);

            _context.Users.Update(user);
            return _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> DeleteAsync(string userId, CancellationToken cancellationToken)
        {
            // IgnoreQueryFilters so an already soft-deleted user is still found,
            // which makes repeated deletes idempotent instead of returning 404.
            var user = await _context.Users
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(u => u.UserId == userId, cancellationToken);

            if (user is null)
            {
                return false;
            }

            if (user.DeletedAt is not null)
            {
                // Already archived; nothing left to do.
                return true;
            }

            // Remove() is rewritten into a soft delete by the
            // SoftDeleteInterceptor (User implements ISoftDeletable), so the row
            // is kept and only stamped with who/when. This avoids the foreign key
            // violation that a physical delete would trigger when orders,
            // reviews, carts, profiles or audit columns still reference the id.
            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            // Only reading data.
            var query = _context.Users.AsNoTracking();

            // Count all matching users before applying pagination.
            var totalCount = await query.CountAsync(cancellationToken);

            // Retrieve only the requested page.
            var users = await query
                .OrderByDescending(user => user.CreatedAt)
                .ThenBy(user => user.UserId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (users, totalCount);
            // Note: The actual pagination occurs in the database,
            // rather than fetching all users first and filtering them in C#.
        }

        // Emails are stored and looked up in a canonical form so that a
        // difference in casing or surrounding whitespace can never create a
        // second account (or hide an existing one during login).
        private static string NormalizeEmail(string email) =>
            email.Trim().ToLowerInvariant();
    }
}