using Dapper;
using Matgar.Application.Abstractions.Dapper;
using Matgar.Application.Abstractions.Queries.ProductVariant;
using Matgar.Application.Features.ProductVariant.Queries.Responses;

namespace Matgar.Infrastructure.Persistence.Queries.ProductVariant
{
    internal sealed class ProductVariantQueries : IProductVariantQueries
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ProductVariantQueries(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IReadOnlyList<ProductVariantResponse>> GetByProductIdAsync(
            Guid productId, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT
                    pv.Id AS VariantId,
                    pv.ProductId,
                    pv.Sku,
                    pv.Price,
                    pv.ImageUrl,
                    pv.AttributesJson,
                    ISNULL(si.QuantityOnHand - si.QuantityReserved, 0) AS AvailableQuantity
                FROM ProductVariants pv
                LEFT JOIN StockItems si ON si.ProductVariantId = pv.Id
                WHERE pv.ProductId = @ProductId AND pv.IsDeleted = 0
                ORDER BY pv.CreatedAt, pv.Id;
                """;

            using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken);

            var rows = await connection.QueryAsync<ProductVariantResponse>(command);
            return rows.AsList();
        }
    }
}