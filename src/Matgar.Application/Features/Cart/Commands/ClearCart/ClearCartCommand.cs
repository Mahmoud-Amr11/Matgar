using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Cart.Commands.ClearCart
{
    public sealed record ClearCartCommand : IRequest<Result>;
}
