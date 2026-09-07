using Matgar.Domain.Entities;

namespace Matgar.Application.Abstractions.Repositories
{

    public interface IUnitOfWork
    {
        IGenericRepository<OutboxMessage> OutboxMessages { get; }
        ICategoryRepository Categories { get; }
        IProductRepository Products { get; }
        IProductVariantRepository ProductVariants { get; }
        IProductReviewRepository ProductReviews { get; }
        ICartRepository Carts { get; }
        ICartItemRepository CartItems { get; }
        IGenericRepository<StockItem> StockItems { get; }
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }


}
