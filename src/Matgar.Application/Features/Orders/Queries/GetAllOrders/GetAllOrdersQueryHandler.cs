using Matgar.Application.Abstractions.Persistence.Queries.Orders;
using Matgar.Application.Common.Pagination;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Orders.Queries.GetAllOrders
{
    public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, Result<PagedResult<GetAllOrdersResponse>>>
    {
        private readonly IOrderQueries _orderQueries;

        public GetAllOrdersQueryHandler(IOrderQueries orderQueries)
        {
            _orderQueries = orderQueries;
        }

        public async Task<Result<PagedResult<GetAllOrdersResponse>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            var offset = (request.Page - 1) * request.PageSize;
            var orders = await _orderQueries.GetAllOrdersAsync(offset, request.PageSize, request.Page, cancellationToken);
            return Result<PagedResult<GetAllOrdersResponse>>.Success(orders);
        }
    }
}
