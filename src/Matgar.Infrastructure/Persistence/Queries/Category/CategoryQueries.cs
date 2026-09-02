using Dapper;
using Matgar.Application.Abstractions.Dapper;
using Matgar.Application.Abstractions.Queries.Category;
using Matgar.Application.Common.Pagination;
using Matgar.Application.Features.Category.Query.Responses;

namespace Matgar.Infrastructure.Persistence.Queries.Category
{

    internal sealed class CategoryQueries : ICategoryQueries
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CategoryQueries(
            IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<PagedResult<CategoryResponse>> GetAllAsync(
            string? search,
            int offset,
            int pageSize,
            int page,
            CancellationToken cancellationToken)
        {

            using var connection =
                _connectionFactory.CreateConnection();

            const string sql = """
            SELECT
                Id AS CategoryId,
                Slug AS CategorySlug,
                Name AS CategoryName
            FROM Categories
            WHERE
                @Search IS NULL
                OR Name LIKE '%' + @Search + '%'
            ORDER BY CreatedAt
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM Categories
            WHERE
                @Search IS NULL
                OR Name LIKE '%' + @Search + '%';
            """;

            var parameters = new
            {
                Search = string.IsNullOrWhiteSpace(search)
                    ? null
                    : search.Trim(),

                Offset = offset,
                PageSize = pageSize
            };

            var command = new CommandDefinition(
                sql,
                parameters,
                cancellationToken: cancellationToken);

            using var multi =
                await connection.QueryMultipleAsync(command);

            var categories =
                (await multi.ReadAsync<CategoryResponse>())
                .ToList();

            var totalCount =
                await multi.ReadSingleAsync<int>();

            return new PagedResult<CategoryResponse>(
                categories,
                page,
                pageSize,
                totalCount);
        }
    }
}