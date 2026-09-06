using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Products.Commands.CreateProduct
{
    public sealed record CreateProductCommand(
       string Name,
       string Description,
       Guid CategoryId) : IRequest<Result<Guid>>;
}