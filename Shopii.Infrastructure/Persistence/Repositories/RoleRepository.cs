using Microsoft.EntityFrameworkCore;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Domain.Entities.Users;

namespace Shopii.Infrastructure.Persistence.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ShopiiDbContext _context;

        public RoleRepository(ShopiiDbContext context)
        {
            _context = context;
        }

        public async Task<Role?> GetByTitleAsync(
            string title,
            CancellationToken cancellationToken = default)
        {
            return await _context.Roles
                .SingleOrDefaultAsync(
                    r => r.RoleTitle == title,
                    cancellationToken);
        }
    }
}