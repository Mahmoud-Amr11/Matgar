namespace Matgar.Application.Abstractions.Persistence.Queries.ProductReview
{
    public interface IPurchaseVerificationQueries
    {
        Task<Guid?> FindReviewableDeliveredOrderIdAsync(
            Guid userId,
            Guid productId,
            CancellationToken cancellationToken);
    }
}
