using Dapper;
using Matgar.Application.Abstractions.Dapper;
using Matgar.Application.Abstractions.Queries.Products;
using Matgar.Application.Common.Pagination;
using Matgar.Application.Features.Products.Queries.Responses;

namespace Matgar.Infrastructure.Persistence.Queries.Produtcs
{
    internal sealed class ProductQueries : IProductQueries
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ProductQueries(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<PagedResult<ProductListItemResponse>> GetAllAsync(
            string? search,
            Guid? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            ProductStatus? status,
            int offset,
            int pageSize,
            int page,
            CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();


            const string sql = """
                WITH ProductPriceRange AS (
                    SELECT
                        pv.ProductId,
                        MIN(pv.Price) AS MinPrice,
                        MAX(pv.Price) AS MaxPrice
                    FROM ProductVariants pv
                    WHERE pv.IsDeleted = 0
                    GROUP BY pv.ProductId
                ),
                ProductThumbnail AS (
                    SELECT
                        pv.ProductId,
                        pv.ImageUrl,
                        ROW_NUMBER() OVER (PARTITION BY pv.ProductId ORDER BY pv.CreatedAt) AS RowNum
                    FROM ProductVariants pv
                    WHERE pv.IsDeleted = 0 AND pv.ImageUrl IS NOT NULL
                )
                SELECT
                    p.Id AS ProductId,
                    p.Name AS ProductName,
                    c.Name AS CategoryName,
                    c.Id AS CategoryId,
                    p.Status,
                    ppr.MinPrice,
                    ppr.MaxPrice,
                    pt.ImageUrl AS ThumbnailUrl,
                    COUNT(*) OVER() AS TotalCount
                FROM Products p
                INNER JOIN Categories c ON c.Id = p.CategoryId
                LEFT JOIN ProductPriceRange ppr ON ppr.ProductId = p.Id
                LEFT JOIN ProductThumbnail pt ON pt.ProductId = p.Id AND pt.RowNum = 1
                WHERE
                    (p.IsDeleted = 0 OR p.IsDeleted IS NULL)
                    AND (@Search IS NULL OR p.Name LIKE '%' + @Search + '%')
                    AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
                    AND (@Status IS NULL OR p.Status = @Status)
                    AND (@MinPrice IS NULL OR ppr.MinPrice >= @MinPrice)
                    AND (@MaxPrice IS NULL OR ppr.MaxPrice <= @MaxPrice)
                ORDER BY p.CreatedAt DESC, p.Id
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                """;

            var parameters = new
            {
                Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Status = status,
                Offset = offset,
                PageSize = pageSize
            };

            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            var rows = (await connection.QueryAsync<ProductRow>(command)).AsList();

            var totalCount = rows.Count > 0 ? rows[0].TotalCount : 0;

            var items = rows.Select(r => new ProductListItemResponse(
                r.ProductId,
                r.ProductName,
                r.CategoryName,
                r.CategoryId,
                r.Status,
                r.MinPrice ?? 0,
                r.MaxPrice ?? 0,
                r.ThumbnailUrl)).ToList();

            return new PagedResult<ProductListItemResponse>(items, page, pageSize, totalCount);
        }

        // ProductRow: shape الصف الخام القادم من SQL — منفصل عن
        // ProductListItemResponse عشان TotalCount (تفصيلة خاصة بالـ paging)
        // متتسربش لداخل الـ DTO العام اللي بيوصل للـ client.
        private sealed record ProductRow(
            Guid ProductId,
            string ProductName,
            string CategoryName,
            Guid CategoryId,
            ProductStatus Status,
            decimal? MinPrice,
            decimal? MaxPrice,
            string? ThumbnailUrl,
            int TotalCount);

        public async Task<ProductDetailsResponse?> GetByIdAsync(
     Guid id, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            // 3 result sets منفصلة في نفس الـ round-trip:
            // 1) المنتج + الفئة + متوسط التقييم وعددها
            // 2) الـ Variants مع الكمية المتاحة (Available = OnHand - Reserved)
            // 3) الـ Reviews
            //
            // لازم يكونوا منفصلين: لو عملنا JOIN مباشر بين Product و Variants
            // و Reviews في استعلام واحد، أي منتج عنده 3 variants و5 reviews
            // هيرجع 15 صف (3 × 5) بدل 8 -- Cartesian product كلاسيكي.
            const string sql = """
                SELECT
                    p.Id AS ProductId,
                    p.Name AS ProductName,
                    p.Description,
                    p.Status,
                    c.Id AS CategoryId,
                    c.Name AS CategoryName,
                    ISNULL(AVG(CAST(pr.Rating AS FLOAT)), 0) AS AverageRating,
                    COUNT(pr.Id) AS ReviewsCount
                FROM Products p
                INNER JOIN Categories c ON c.Id = p.CategoryId
                LEFT JOIN ProductReviews pr ON pr.ProductId = p.Id
                WHERE p.Id = @Id AND (p.IsDeleted = 0 OR p.IsDeleted IS NULL)
                GROUP BY p.Id, p.Name, p.Description, p.Status, c.Id, c.Name;

                SELECT
                    pv.Id AS VariantId,
                    pv.Sku,
                    pv.Price,
                    pv.ImageUrl,
                    pv.AttributesJson,
                    ISNULL(si.QuantityOnHand - si.QuantityReserved, 0) AS AvailableQuantity
                FROM ProductVariants pv
                LEFT JOIN StockItems si ON si.ProductVariantId = pv.Id
                WHERE pv.ProductId = @Id AND pv.IsDeleted = 0;

                SELECT
                    pr.Id AS ReviewId,
                    pr.UserId,
                    pr.Rating,
                    pr.Comment,
                    pr.CreatedAt
                FROM ProductReviews pr
                WHERE pr.ProductId = @Id
                ORDER BY pr.CreatedAt DESC;
                """;

            var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);

            using var multi = await connection.QueryMultipleAsync(command);

            var productRow = await multi.ReadSingleOrDefaultAsync<ProductDetailsRow>();
            if (productRow is null)
                return null;

            var variants = (await multi.ReadAsync<ProductVariantResponse>()).ToList();
            var reviews = (await multi.ReadAsync<ProductReviewResponse>()).ToList();

            return new ProductDetailsResponse(
                productRow.ProductId,
                productRow.ProductName,
                productRow.Description,
                productRow.Status,
                productRow.CategoryId,
                productRow.CategoryName,
                variants,
                reviews,
                productRow.AverageRating,
                productRow.ReviewsCount);
        }

        private sealed record ProductDetailsRow(
            Guid ProductId,
            string ProductName,
            string Description,
            ProductStatus Status,
            Guid CategoryId,
            string CategoryName,
            double AverageRating,
            int ReviewsCount);
    }
}
