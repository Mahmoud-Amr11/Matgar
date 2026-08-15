using System.Linq.Expressions;

namespace Matgar.Application.Abstractions.Repositories
{
    public interface IGenericRepository<T>
    {
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        void Update(T entity);
        void Remove(T entity);
    }
}
