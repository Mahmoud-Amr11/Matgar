namespace Matgar.Application.Abstractions.Queries.ProductReview
{
    public interface IPurchaseVerificationQueries
    {
        Task<Guid?> FindReviewableDeliveredOrderIdAsync(
            Guid userId,
            Guid productId,
            CancellationToken cancellationToken);
    }
}
