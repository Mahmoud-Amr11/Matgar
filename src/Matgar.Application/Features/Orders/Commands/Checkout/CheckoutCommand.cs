using MediatR;
using Matgar.Application.Common.Results;

namespace Matgar.Application.Features.Orders.Commands.Checkout
{
    public sealed record CheckoutCommand(Guid AddressId, string? CouponCode) : IRequest<Result<Guid>>;
}
