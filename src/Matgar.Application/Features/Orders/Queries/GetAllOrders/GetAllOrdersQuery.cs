using MediatR;
using Matgar.Application.Common.Results;

namespace Matgar.Application.Features.Orders.Queries.GetAllOrders
{
    public sealed record GetAllOrdersQuery() : IRequest<Result<IReadOnlyList<GetAllOrdersResponse>>>;
}
