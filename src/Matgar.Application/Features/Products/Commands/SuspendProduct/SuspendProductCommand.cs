using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Products.Commands.SuspendProduct
{
    public sealed record SuspendProductCommand(Guid ProductId, string Reason) : IRequest<Result>;
}
