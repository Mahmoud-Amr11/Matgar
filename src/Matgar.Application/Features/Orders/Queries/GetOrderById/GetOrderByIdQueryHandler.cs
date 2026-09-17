using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Orders.Queries.GetOrderById
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailResponse>>
    {
        private readonly Matgar.Application.Abstractions.Queries.Orders.IOrderQueries _orderQueries;
        private readonly ICurrentUserService _currentUser;

        public GetOrderByIdQueryHandler(Matgar.Application.Abstractions.Queries.Orders.IOrderQueries orderQueries, ICurrentUserService currentUser)
        {
            _orderQueries = orderQueries;
            _currentUser = currentUser;
        }

        public async Task<Result<OrderDetailResponse>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var userId))
                return Error.Unauthorized(message: "Invalid user identity.");

            var order = await _orderQueries.GetOrderByIdAsync(request.OrderId, cancellationToken);

            if (order == null || order.Items == null) return Error.NotFound(code: "Order.NotFound", message: "Order not found.");

            // Authorization: admin can view any order, a customer only their own,
            // and a vendor only orders that contain one of their products.
            if (!_currentUser.IsInRole("Admin") && order.CustomerId != userId)
            {
                if (!_currentUser.IsInRole("Vendor"))
                    return Error.Forbidden(code: "Order.Forbidden", message: "You do not have permission to view this order.");

                var vendorOwnsOrder = await _orderQueries.VendorOwnsOrderAsync(userId, request.OrderId, cancellationToken);
                if (!vendorOwnsOrder)
                    return Error.Forbidden(code: "Order.Forbidden", message: "You do not have permission to view this order.");
            }

            return Result<OrderDetailResponse>.Success(order);
        }
    }
}
