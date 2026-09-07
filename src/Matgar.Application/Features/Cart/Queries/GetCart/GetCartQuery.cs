using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Cart.Queries.GetCart
{
    public sealed record GetCartQuery : IRequest<Result<Responses.CartResponse>>;
}
