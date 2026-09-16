using Dapper;
using Matgar.Application.Abstractions.Dapper;
using Matgar.Application.Abstractions.Queries.Orders;
using Matgar.Application.Common.Pagination;
using Matgar.Application.Features.Orders.Queries.GetAllOrders;
using Matgar.Application.Features.Orders.Queries.GetOrderById;
using Matgar.Application.Features.Orders.Queries.GetOrders;
using Matgar.Application.Features.Orders.Queries.GetVendorOrders;

namespace Matgar.Infrastructure.Persistence.Queries.Orders
{
    internal sealed class OrderQueries : IOrderQueries
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public OrderQueries(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<PagedResult<OrderResponse>> GetOrdersByUserIdAsync(Guid userId, int offset, int pageSize, int page, CancellationToken cancellationToken = default)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT Id, CreatedAt, SubTotal, DiscountAmount, TotalAmount, Status,
                COUNT(*) OVER() AS TotalCount
                FROM Orders
                WHERE CustomerId = @UserId
                ORDER BY CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                """;

            var rows = (await connection.QueryAsync<OrderRowPaged>(new CommandDefinition(sql, new { UserId = userId, Offset = offset, PageSize = pageSize }, cancellationToken: cancellationToken))).AsList();

            var totalCount = rows.Count > 0 ? rows[0].TotalCount : 0;

            var items = rows.Select(r => new OrderResponse
            {
                Id = r.Id,
                CreatedAt = r.CreatedAt,
                SubTotal = r.SubTotal,
                DiscountAmount = r.DiscountAmount,
                TotalAmount = r.TotalAmount,
                Status = r.Status.ToString()
            }).ToList();

            return new PagedResult<OrderResponse>(items, page, pageSize, totalCount);
        }

        public async Task<OrderDetailResponse?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string orderSql = """
                SELECT Id, CreatedAt, SubTotal, DiscountAmount, TotalAmount, Status, ShippingAddressSnapshot
                FROM Orders
                WHERE Id = @OrderId
                """;

            var order = await connection.QuerySingleOrDefaultAsync<OrderRowDetail>(new CommandDefinition(orderSql, new { OrderId = orderId }, cancellationToken: cancellationToken));
            if (order == null) return null;

            const string itemsSql = """
                SELECT oi.ProductVariantId, oi.Quantity, oi.UnitPrice, pv.Sku
                FROM OrderItems oi
                LEFT JOIN ProductVariants pv ON pv.Id = oi.ProductVariantId
                WHERE oi.OrderId = @OrderId
                """;

            var items = (await connection.QueryAsync<OrderItemRow>(new CommandDefinition(itemsSql, new { OrderId = orderId }, cancellationToken: cancellationToken))).ToList();

            var response = new OrderDetailResponse
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                SubTotal = order.SubTotal,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                ShippingAddressSnapshot = order.ShippingAddressSnapshot,
                Items = items.Select(i => new OrderItemResponse
                {
                    ProductVariantId = i.ProductVariantId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Sku = i.Sku
                }).ToList()
            };

            return response;
        }

        public async Task<PagedResult<VendorOrderResponse>> GetOrdersByVendorIdAsync(Guid vendorId, int offset, int pageSize, int page, CancellationToken cancellationToken = default)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT DISTINCT o.Id, o.CreatedAt, o.SubTotal, o.DiscountAmount, o.TotalAmount, o.Status, o.CustomerId,
                COUNT(*) OVER() AS TotalCount
                FROM Orders o
                INNER JOIN OrderItems oi ON oi.OrderId = o.Id
                INNER JOIN ProductVariants pv ON pv.Id = oi.ProductVariantId
                INNER JOIN Products p ON p.Id = pv.ProductId
                WHERE p.VendorId = @VendorId
                ORDER BY o.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                """;

            var rows = (await connection.QueryAsync<VendorOrderRowPaged>(new CommandDefinition(sql, new { VendorId = vendorId, Offset = offset, PageSize = pageSize }, cancellationToken: cancellationToken))).AsList();

            var totalCount = rows.Count > 0 ? rows[0].TotalCount : 0;

            var items = rows.Select(r => new VendorOrderResponse
            {
                Id = r.Id,
                CreatedAt = r.CreatedAt,
                SubTotal = r.SubTotal,
                DiscountAmount = r.DiscountAmount,
                TotalAmount = r.TotalAmount,
                Status = r.Status.ToString(),
                CustomerId = r.CustomerId
            }).ToList();

            return new PagedResult<VendorOrderResponse>(items, page, pageSize, totalCount);
        }

        public async Task<PagedResult<GetAllOrdersResponse>> GetAllOrdersAsync(int offset, int pageSize, int page, CancellationToken cancellationToken = default)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT o.Id, o.CreatedAt, o.SubTotal, o.DiscountAmount, o.TotalAmount, o.Status, o.CustomerId,
                COUNT(*) OVER() AS TotalCount
                FROM Orders o
                ORDER BY o.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                """;

            var rows = (await connection.QueryAsync<GetAllOrdersRowPaged>(new CommandDefinition(sql, new { Offset = offset, PageSize = pageSize }, cancellationToken: cancellationToken))).AsList();

            var totalCount = rows.Count > 0 ? rows[0].TotalCount : 0;

            var items = rows.Select(r => new GetAllOrdersResponse
            {
                Id = r.Id,
                CreatedAt = r.CreatedAt,
                SubTotal = r.SubTotal,
                DiscountAmount = r.DiscountAmount,
                TotalAmount = r.TotalAmount,
                Status = r.Status.ToString(),
                CustomerId = r.CustomerId
            }).ToList();

            return new PagedResult<GetAllOrdersResponse>(items, page, pageSize, totalCount);
        }

        private sealed record OrderRow(Guid Id, DateTime CreatedAt, decimal SubTotal, decimal DiscountAmount, decimal TotalAmount, int Status);
        private sealed record OrderRowPaged(Guid Id, DateTime CreatedAt, decimal SubTotal, decimal DiscountAmount, decimal TotalAmount, int Status, int TotalCount);
        private sealed record OrderRowDetail(Guid Id, DateTime CreatedAt, decimal SubTotal, decimal DiscountAmount, decimal TotalAmount, int Status, string ShippingAddressSnapshot);
        private sealed record OrderItemRow(Guid ProductVariantId, int Quantity, decimal UnitPrice, string Sku);
        private sealed record VendorOrderRow(Guid Id, DateTime CreatedAt, decimal TotalAmount, int Status);
        private sealed record VendorOrderRowPaged(Guid Id, DateTime CreatedAt, decimal SubTotal, decimal DiscountAmount, decimal TotalAmount, int Status, Guid CustomerId, int TotalCount);
        private sealed record GetAllOrdersRowPaged(Guid Id, DateTime CreatedAt, decimal SubTotal, decimal DiscountAmount, decimal TotalAmount, int Status, Guid CustomerId, int TotalCount);
        private sealed record GetAllOrdersRow(Guid Id, DateTime CreatedAt, decimal TotalAmount, int Status);
    }
}
