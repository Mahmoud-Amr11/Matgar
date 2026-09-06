namespace Matgar.Application.Features.Products.Queries.Responses
{
    public sealed record ProductListItemResponse(
          Guid ProductId,
          string ProductName,
          string CategoryName,
          Guid CategoryId,
          ProductStatus Status,
          decimal MinPrice,
          decimal MaxPrice,
          string? ThumbnailUrl);

    public sealed record ProductDetailsResponse(
        Guid ProductId,
        string ProductName,
        string Description,
        ProductStatus Status,
        Guid CategoryId,
        string CategoryName,
        IReadOnlyList<ProductVariantResponse> Variants,
        IReadOnlyList<ProductReviewResponse> Reviews,
        double AverageRating,
        int ReviewsCount);

    public sealed record ProductVariantResponse(
        Guid VariantId,
        string Sku,
        decimal Price,
        string? ImageUrl,
        string AttributesJson,
        int AvailableQuantity);

    public sealed record ProductReviewResponse(
        Guid ReviewId,
        Guid UserId,
        int Rating,
        string? Comment,
        DateTime CreatedAt);
}
