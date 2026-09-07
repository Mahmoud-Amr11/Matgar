using Matgar.Application.Abstractions.Queries.ProductReview;
using Matgar.Application.Common.Pagination;
using Matgar.Application.Common.Results;
using Matgar.Application.Features.ProductReview.Queries.Responses;
using MediatR;

namespace Matgar.Application.Features.ProductReview.Queries.GetProductReviews
{
    public class GetProductReviewsQueryHandler
        : IRequestHandler<GetProductReviewsQuery, Result<PagedResult<ProductReviewListItemResponse>>>
    {
        private readonly IProductReviewQueries _reviewQueries;

        public GetProductReviewsQueryHandler(IProductReviewQueries reviewQueries)
        {
            _reviewQueries = reviewQueries;
        }

        public async Task<Result<PagedResult<ProductReviewListItemResponse>>> Handle(
            GetProductReviewsQuery request, CancellationToken cancellationToken)
        {
            var pagination = new PaginationParams
            {
                Page = request.Page,
                PageSize = request.PageSize
            };

            var result = await _reviewQueries.GetByProductIdAsync(
                request.ProductId,
                pagination.Offset,
                pagination.NormalizedPageSize,
                pagination.NormalizedPage,
                cancellationToken);

            return result;
        }
    }
}

