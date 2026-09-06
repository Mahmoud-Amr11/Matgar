namespace Matgar.Api.Requests.ProductVariants
{
    public sealed record CreateProductVariantRequest(
       string Sku,
       decimal Price,
       string? ImageUrl,
       string AttributesJson,
       int InitialQuantity);
}
