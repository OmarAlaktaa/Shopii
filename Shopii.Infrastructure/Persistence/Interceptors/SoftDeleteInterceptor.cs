using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shopii.Application.Interfaces.Services;
using Shopii.Domain.Common;

namespace Shopii.Infrastructure.Persistence.Interceptors
{
    /// <summary>
    /// Converts a physical delete of an <see cref="ISoftDeletable"/> entity into
    /// a soft delete: the entity is kept, stamped with the deletion time and the
    /// current user, and saved as an update instead of being removed.
    /// </summary>
    public class SoftDeleteInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUserService;

        public SoftDeleteInterceptor(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            ApplySoftDelete(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ApplySoftDelete(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void ApplySoftDelete(DbContext? context)
        {
            if (context is null)
            {
                return;
            }

            foreach (var entry in context.ChangeTracker.Entries<ISoftDeletable>())
            {
                if (entry.State != EntityState.Deleted)
                {
                    continue;
                }

                // Rewrite the delete into an update of the audit fields. The rest
                // of the entity (including the user's email) is left untouched so
                // the archived row keeps a faithful record of the real account,
                // which is the whole point of a soft delete.
                entry.State = EntityState.Modified;
                entry.Entity.DeletedAt = DateTime.UtcNow;
                entry.Entity.DeletedByUserId = _currentUserService.UserId;
            }
        }
    }
}
