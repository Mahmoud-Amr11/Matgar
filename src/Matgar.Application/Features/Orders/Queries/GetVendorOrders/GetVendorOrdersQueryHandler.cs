using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Common.Pagination;
using Matgar.Application.Common.Results;
using Matgar.Application.Common.Pagination;
using MediatR;

namespace Matgar.Application.Features.Orders.Queries.GetVendorOrders
{
    public class GetVendorOrdersQueryHandler : IRequestHandler<GetVendorOrdersQuery, Result<PagedResult<VendorOrderResponse>>>
    {
        private readonly Matgar.Application.Abstractions.Queries.Orders.IOrderQueries _orderQueries;
        private readonly ICurrentUserService _currentUser;

        public GetVendorOrdersQueryHandler(Matgar.Application.Abstractions.Queries.Orders.IOrderQueries orderQueries, ICurrentUserService currentUser)
        {
            _orderQueries = orderQueries;
            _currentUser = currentUser;
        }

        public async Task<Result<PagedResult<VendorOrderResponse>>> Handle(GetVendorOrdersQuery request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var vendorId))
                return Error.Unauthorized(message: "Invalid vendor identity.");

            var offset = (request.Page - 1) * request.PageSize;
            var orders = await _orderQueries.GetOrdersByVendorIdAsync(vendorId, offset, request.PageSize, request.Page, cancellationToken);
            return Result<PagedResult<VendorOrderResponse>>.Success(orders);
        }
    }
}
