using Matgar.Application.Abstractions.Repositories;
using Matgar.Domain.Entities;
using Matgar.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore.Storage;

namespace Matgar.Infrastructure.Persistence.Repositories
{
    internal sealed class UnitOfWork : IUnitOfWork, IAsyncDisposable
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        private IGenericRepository<OutboxMessage>? _outboxMessages;
        public IGenericRepository<OutboxMessage> OutboxMessages =>
            _outboxMessages ??= new GenericRepository<OutboxMessage>(_context);

        private ICategoryRepository? _categories;
        public ICategoryRepository Categories =>
            _categories ??= new CategoryRepository(_context);


        private IProductRepository? _products;
        public IProductRepository Products =>
            _products ??= new ProductRepository(_context);

        private IProductVariantRepository? _productVariants;
        public IProductVariantRepository ProductVariants =>
            _productVariants ??= new ProductVariantRepository(_context);


        private IProductReviewRepository? _productReviews;
        public IProductReviewRepository ProductReviews =>
            _productReviews ??= new ProductReviewRepository(_context);

        private ICartRepository? _carts;
        public ICartRepository Carts =>
            _carts ??= new CartRepository(_context);

        private ICartItemRepository? _cartItems;
        public ICartItemRepository CartItems =>
            _cartItems ??= new CartItemRepository(_context);

        private IGenericRepository<StockItem>? _stockItems;
        public IGenericRepository<StockItem> StockItems =>
            _stockItems ??= new GenericRepository<StockItem>(_context);

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is not null)
                return; // already in a transaction, avoid overwriting the reference

            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is null)
                throw new InvalidOperationException("No active transaction to commit.");

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await _transaction.RollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is null)
                return;

            try
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);

        public async ValueTask DisposeAsync()
        {
            if (_transaction is not null)
            {
                await _transaction.DisposeAsync();
            }
        }
    }
}