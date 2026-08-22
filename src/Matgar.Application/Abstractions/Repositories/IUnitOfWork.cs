using Matgar.Domain.Entities;

namespace Matgar.Application.Abstractions.Repositories
{

    public interface IUnitOfWork
    {
        IGenericRepository<OutboxMessage> OutboxMessages { get; }
        ICategoryRepository Categories { get; }

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }


}
