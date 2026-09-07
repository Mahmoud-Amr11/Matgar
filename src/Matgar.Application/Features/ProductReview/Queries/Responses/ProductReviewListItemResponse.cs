namespace Matgar.Application.Features.ProductReview.Queries.Responses
{
    public sealed record ProductReviewListItemResponse(
          Guid ReviewId,
          Guid UserId,
          int Rating,
          string? Comment,
          DateTime CreatedAt);
}
