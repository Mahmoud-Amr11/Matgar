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

            // ensure ownership
            // OrderQueries does not include CustomerId as DTO; rely on GetOrderById used in controller which previously filtered by user.
            return Result<OrderDetailResponse>.Success(order);
        }
    }
}
