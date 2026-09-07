using Dapper;
using Matgar.Application.Abstractions.Dapper;
using Matgar.Application.Abstractions.Queries.ProductReview;

namespace Matgar.Infrastructure.Persistence.Queries.ProductReview
{
    internal sealed class PurchaseVerificationQueries : IPurchaseVerificationQueries
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public PurchaseVerificationQueries(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Guid?> FindReviewableDeliveredOrderIdAsync(
            Guid userId, Guid productId, CancellationToken cancellationToken)
        {
            const string sql = """
    SELECT TOP 1 o.Id
    FROM Orders o
    INNER JOIN OrderItems oi ON oi.OrderId = o.Id
    INNER JOIN ProductVariants pv ON pv.Id = oi.ProductVariantId
    LEFT JOIN ProductReviews pr
        ON pr.OrderId = o.Id
        AND pr.ProductId = pv.ProductId
        AND pr.UserId = o.CustomerId
    WHERE
        o.CustomerId = @UserId
        AND pv.ProductId = @ProductId
        AND o.Status = @DeliveredStatus
        AND pr.Id IS NULL
    ORDER BY o.DeliveredAt ASC;
    """;

            using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                sql,
                new { UserId = userId, ProductId = productId, DeliveredStatus = OrderStatus.Delivered },
                cancellationToken: cancellationToken);

            return await connection.QuerySingleOrDefaultAsync<Guid?>(command);
        }
    }
}
