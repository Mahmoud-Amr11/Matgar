using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Products.Commands.DeleteProduct
{
    public sealed record DeleteProductCommand(Guid ProductId) : IRequest<Result>;
}
