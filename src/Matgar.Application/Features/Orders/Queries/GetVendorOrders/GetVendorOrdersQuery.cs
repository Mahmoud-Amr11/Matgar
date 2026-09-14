using MediatR;
using Matgar.Application.Common.Results;

namespace Matgar.Application.Features.Orders.Queries.GetVendorOrders
{
    public sealed record GetVendorOrdersQuery() : IRequest<Result<IReadOnlyList<VendorOrderResponse>>>;
}
