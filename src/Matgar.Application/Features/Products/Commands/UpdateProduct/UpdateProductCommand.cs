using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Products.Commands.UpdateProduct
{
    public sealed record UpdateProductCommand(
        Guid ProductId,
        string Name,
        string Description,
        Guid CategoryId) : IRequest<Result>;
}
