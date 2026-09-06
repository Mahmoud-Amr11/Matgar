namespace Matgar.Api.Requests.ProductVariants
{
    public sealed record UpdateProductVariantRequest(
       decimal Price,
       string? ImageUrl,
       string AttributesJson);
}
