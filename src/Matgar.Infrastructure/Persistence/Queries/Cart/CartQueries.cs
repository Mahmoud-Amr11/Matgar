using Dapper;
using Matgar.Application.Abstractions.Dapper;
using Matgar.Application.Abstractions.Queries.Cart;
using Matgar.Application.Features.Cart.Queries.Responses;

namespace Matgar.Infrastructure.Persistence.Queries.Cart
{
    internal sealed class CartQueries : ICartQueries
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CartQueries(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<CartResponse?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string cartSql = """
                SELECT
                    c.Id AS CartId,
                    c.UserId,
                    c.CreatedAt
                FROM Carts c
                WHERE c.UserId = @UserId
                  AND (c.IsDeleted = 0 OR c.IsDeleted IS NULL);
                """;

            var cartRow = await connection.QuerySingleOrDefaultAsync<CartRow>(
                new CommandDefinition(cartSql, new { UserId = userId }, cancellationToken: cancellationToken));

            if (cartRow is null)
                return null;

            const string itemsSql = """
                SELECT
                    ci.Id AS ItemId,
                    ci.ProductVariantId,
                    pv.Sku,
                    p.Name AS ProductName,
                    pv.ImageUrl,
                    pv.AttributesJson,
                    ci.PriceSnapshot AS UnitPrice,
                    ci.Quantity,
                    ci.PriceSnapshot * ci.Quantity AS LineTotal
                FROM CartItems ci
                INNER JOIN ProductVariants pv ON pv.Id = ci.ProductVariantId
                INNER JOIN Products p ON p.Id = pv.ProductId
                WHERE ci.CartId = @CartId
                ORDER BY ci.Id;
                """;

            var items = (await connection.QueryAsync<CartItemResponse>(
                new CommandDefinition(itemsSql, new { CartId = cartRow.CartId }, cancellationToken: cancellationToken))).ToList();

            return new CartResponse(
                cartRow.CartId,
                cartRow.UserId,
                cartRow.CreatedAt,
                items,
                items.Sum(i => i.LineTotal));
        }

        private sealed record CartRow(
            Guid CartId,
            Guid? UserId,
            DateTime CreatedAt);
    }
}