using Matgar.Domain.Entities;

namespace Matgar.Application.Abstractions.Persistence.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Order?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);
    }
}
