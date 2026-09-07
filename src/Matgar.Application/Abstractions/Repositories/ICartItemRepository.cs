using Matgar.Domain.Entities;

namespace Matgar.Application.Abstractions.Repositories
{
    public interface ICartItemRepository : IGenericRepository<CartItem>
    {
        Task<CartItem?> GetByCartAndVariantAsync(Guid cartId, Guid productVariantId, CancellationToken cancellationToken = default);
    }
}
