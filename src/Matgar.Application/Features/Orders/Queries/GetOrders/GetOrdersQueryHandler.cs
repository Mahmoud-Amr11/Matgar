using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using Matgar.Application.Common.Pagination;
using MediatR;

namespace Matgar.Application.Features.Orders.Queries.GetOrders
{
    public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, Result<PagedResult<OrderResponse>>>
    {
        private readonly Matgar.Application.Abstractions.Queries.Orders.IOrderQueries _orderQueries;
        private readonly ICurrentUserService _currentUser;

        public GetOrdersQueryHandler(Matgar.Application.Abstractions.Queries.Orders.IOrderQueries orderQueries, ICurrentUserService currentUser)
        {
            _orderQueries = orderQueries;
            _currentUser = currentUser;
        }

        public async Task<Result<PagedResult<OrderResponse>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var userId))
                return Error.Unauthorized(message: "Invalid user identity.");

            var offset = (request.Page - 1) * request.PageSize;
            var orders = await _orderQueries.GetOrdersByUserIdAsync(userId, offset, request.PageSize, request.Page, cancellationToken);
            return Result<PagedResult<OrderResponse>>.Success(orders);
        }
    }
}
