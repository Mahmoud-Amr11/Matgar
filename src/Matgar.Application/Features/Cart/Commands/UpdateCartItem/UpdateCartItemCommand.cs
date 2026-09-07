using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Cart.Commands.UpdateCartItem
{
    public sealed record UpdateCartItemCommand(
        Guid CartItemId,
        int Quantity) : IRequest<Result>;
}
