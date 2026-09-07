using Matgar.Application.Abstractions.Repositories;
using Matgar.Domain.Entities;
using Matgar.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Matgar.Infrastructure.Persistence.Repositories
{
    internal sealed class CartItemRepository : GenericRepository<CartItem>, ICartItemRepository
    {
        private readonly ApplicationDbContext _context;

        public CartItemRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<CartItem?> GetByCartAndVariantAsync(Guid cartId, Guid productVariantId, CancellationToken cancellationToken = default)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductVariantId == productVariantId, cancellationToken);
        }
    }
}
