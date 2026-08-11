using Matgar.Application.Abstractions.Repositories;
using Matgar.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore.Storage;

namespace Matgar.Infrastructure.Persistence.Repositories
{
    internal sealed class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        private IDbContextTransaction? _transaction;


        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }



        public async Task BeginTransactionAsync(
            CancellationToken cancellationToken = default)
        {
            _transaction =
                await _context.Database
                .BeginTransactionAsync(cancellationToken);
        }



        public async Task CommitTransactionAsync(
            CancellationToken cancellationToken = default)
        {
            await _transaction!
                .CommitAsync(cancellationToken);
        }


        public async Task RollbackTransactionAsync(
            CancellationToken cancellationToken = default)
        {
            await _transaction!
                .RollbackAsync(cancellationToken);
        }
    }
}
