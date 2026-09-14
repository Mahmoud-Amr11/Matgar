using MediatR;
using Matgar.Application.Common.Results;

namespace Matgar.Application.Features.Orders.Commands.UpdateVendorOrderStatus
{
    public sealed record UpdateVendorOrderStatusCommand(Guid OrderId, int NewStatus) : IRequest<Result>;
}
