using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.ProductVariant.Commands.CreateProductVariant
{
    public sealed record CreateProductVariantCommand(
      Guid ProductId,
      string Sku,
      decimal Price,
      string? ImageUrl,
      string AttributesJson,
      int InitialQuantity) : IRequest<Result<Guid>>;
}
