using Matgar.Application.Common.Pagination;
using Matgar.Application.Features.ProductReview.Queries.Responses;

namespace Matgar.Application.Abstractions.Queries.ProductReview
{
    public interface IProductReviewQueries
    {
        Task<PagedResult<ProductReviewListItemResponse>> GetByProductIdAsync(
            Guid productId,
            int offset,
            int pageSize,
            int page,
            CancellationToken cancellationToken);
    }
}
