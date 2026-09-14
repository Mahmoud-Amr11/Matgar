using MediatR;
using Matgar.Application.Common.Results;

namespace Matgar.Application.Features.Orders.Queries.GetOrderById
{
    public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<Result<OrderDetailResponse>>;
}
