using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Cart.Commands.RemoveCartItem
{
    public sealed record RemoveCartItemCommand(Guid CartItemId) : IRequest<Result>;
}
