using Matgar.Domain.Entities;

namespace Matgar.Application.Abstractions.Persistence.Repositories
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
        ICouponRepository Coupons { get; }
        IOrderRepository Orders { get; }
        IProcessedWebhookRepository ProcessedWebhooks { get; }
        IGenericRepository<StockItem> StockItems { get; }
        IGenericRepository<OrderItem> OrderItems { get; }
        IGenericRepository<Address> Addresses { get; }
        IGenericRepository<NotificationLog> Notifications { get; }
        IGenericRepository<Payment> Payments { get; }

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
