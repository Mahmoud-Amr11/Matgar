using Matgar.Domain.Entities;

namespace Matgar.Application.Abstractions.Persistence.Repositories
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
