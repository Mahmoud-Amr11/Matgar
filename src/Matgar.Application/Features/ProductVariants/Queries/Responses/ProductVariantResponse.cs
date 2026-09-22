namespace Matgar.Application.Features.ProductVariant.Queries.Responses
{
    public sealed record ProductVariantResponse(
      Guid VariantId,
      Guid ProductId,
      string Sku,
      decimal Price,
      string? ImageUrl,
      string AttributesJson,
      int AvailableQuantity);
}
