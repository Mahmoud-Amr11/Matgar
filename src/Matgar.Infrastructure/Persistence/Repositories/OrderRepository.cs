using Matgar.Application.Abstractions.Persistence.Repositories;
using Matgar.Domain.Entities;
using Matgar.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Matgar.Infrastructure.Persistence.Repositories
{
    internal class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.ProductVariant)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }

        public async Task<Order?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
        {
            if (Guid.TryParse(reference, out var guidId))
            {
                return await GetByIdAsync(guidId, cancellationToken);
            }
            return null;
        }
    }
}
