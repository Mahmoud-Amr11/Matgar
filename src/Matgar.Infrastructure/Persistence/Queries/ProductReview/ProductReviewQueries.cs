using Dapper;
using Matgar.Application.Abstractions.Dapper;
using Matgar.Application.Abstractions.Queries.ProductReview;
using Matgar.Application.Common.Pagination;
using Matgar.Application.Features.ProductReview.Queries.Responses;

namespace Matgar.Infrastructure.Persistence.Queries.ProductReview
{
    internal sealed class ProductReviewQueries : IProductReviewQueries
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ProductReviewQueries(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<PagedResult<ProductReviewListItemResponse>> GetByProductIdAsync(
            Guid productId, int offset, int pageSize, int page, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT
                    pr.Id AS ReviewId,
                    pr.UserId,
                    pr.Rating,
                    pr.Comment,
                    pr.CreatedAt,
                    COUNT(*) OVER() AS TotalCount
                FROM ProductReviews pr
                WHERE pr.ProductId = @ProductId
                ORDER BY pr.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                """;

            using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                sql,
                new { ProductId = productId, Offset = offset, PageSize = pageSize },
                cancellationToken: cancellationToken);

            var rows = (await connection.QueryAsync<ReviewRow>(command)).AsList();

            var totalCount = rows.Count > 0 ? rows[0].TotalCount : 0;

            var items = rows.Select(r => new ProductReviewListItemResponse(
                r.ReviewId, r.UserId, r.Rating, r.Comment, r.CreatedAt)).ToList();

            return new PagedResult<ProductReviewListItemResponse>(items, page, pageSize, totalCount);
        }

        private sealed record ReviewRow(
            Guid ReviewId, Guid UserId, int Rating, string? Comment, DateTime CreatedAt, int TotalCount);
    }
}
