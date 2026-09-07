namespace Matgar.Api.Requests.ProductReview
{
    public sealed record CreateProductReviewRequest(int Rating, string? Comment);
}
