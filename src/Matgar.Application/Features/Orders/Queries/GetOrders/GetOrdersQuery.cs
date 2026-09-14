using MediatR;
using Matgar.Application.Common.Results;

namespace Matgar.Application.Features.Orders.Queries.GetOrders
{
    public sealed record GetOrdersQuery() : IRequest<Result<IReadOnlyList<OrderResponse>>>;
}
