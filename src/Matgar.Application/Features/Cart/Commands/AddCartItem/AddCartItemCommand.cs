using Matgar.Application.Common.Results;
using Matgar.Application.Features.Cart.Queries.Responses;
using MediatR;

namespace Matgar.Application.Features.Cart.Commands.AddCartItem
{
    public sealed record AddCartItemCommand(
        Guid ProductVariantId,
        int Quantity) : IRequest<Result<AddCartItemResponse>>;
}
