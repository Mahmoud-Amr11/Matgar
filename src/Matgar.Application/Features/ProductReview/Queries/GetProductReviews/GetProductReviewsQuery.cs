using Matgar.Application.Common.Caching;
using Matgar.Application.Common.Pagination;
using Matgar.Application.Features.ProductReview.Queries.Responses;

namespace Matgar.Application.Features.ProductReview.Queries.GetProductReviews
{
    public sealed record GetProductReviewsQuery(
       Guid ProductId,
       int Page = 1,
       int PageSize = 20) : ICacheableQuery<PagedResult<ProductReviewListItemResponse>>
    {
        public string CacheKey => $"GetProductReviews_{ProductId}_Page_{Page}_Size_{PageSize}";

        public TimeSpan? Expiration => TimeSpan.FromMinutes(5);

        public bool BypassCache => false;
    }
}

