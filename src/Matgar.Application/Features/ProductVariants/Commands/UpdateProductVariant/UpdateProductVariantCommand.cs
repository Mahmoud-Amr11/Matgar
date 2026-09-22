using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.ProductVariant.Commands.UpdateProductVariant
{
    public sealed record UpdateProductVariantCommand(
       Guid ProductId,
       Guid VariantId,
       decimal Price,
       string? ImageUrl,
       string AttributesJson) : IRequest<Result>;
}
