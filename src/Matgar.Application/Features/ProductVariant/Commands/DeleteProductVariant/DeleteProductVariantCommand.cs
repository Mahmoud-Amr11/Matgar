using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.ProductVariant.Commands.DeleteProductVariant
{
    public sealed record DeleteProductVariantCommand(Guid ProductId, Guid VariantId) : IRequest<Result>;
}
