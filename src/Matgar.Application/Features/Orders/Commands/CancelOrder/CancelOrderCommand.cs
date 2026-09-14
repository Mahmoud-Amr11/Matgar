using MediatR;
using Matgar.Application.Common.Results;

namespace Matgar.Application.Features.Orders.Commands.CancelOrder
{
    public sealed record CancelOrderCommand(Guid OrderId) : IRequest<Result>;
}
