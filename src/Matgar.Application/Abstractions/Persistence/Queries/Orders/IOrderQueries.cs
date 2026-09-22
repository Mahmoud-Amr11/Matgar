using Matgar.Application.Features.Orders.Queries.GetAllOrders;
using Matgar.Application.Features.Orders.Queries.GetOrderById;
using Matgar.Application.Common.Pagination;
using Matgar.Application.Features.Orders.Queries.GetOrders;
using Matgar.Application.Features.Orders.Queries.GetVendorOrders;

namespace Matgar.Application.Abstractions.Persistence.Queries.Orders
{
    public interface IOrderQueries
    {
        Task<PagedResult<OrderResponse>> GetOrdersByUserIdAsync(Guid userId, int offset, int pageSize, int page, CancellationToken cancellationToken = default);
        Task<OrderDetailResponse?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task<PagedResult<VendorOrderResponse>> GetOrdersByVendorIdAsync(Guid vendorId, int offset, int pageSize, int page, CancellationToken cancellationToken = default);
        Task<PagedResult<GetAllOrdersResponse>> GetAllOrdersAsync(int offset, int pageSize, int page, CancellationToken cancellationToken = default);
        Task<bool> VendorOwnsOrderAsync(Guid vendorId, Guid orderId, CancellationToken cancellationToken = default);
    }
}
