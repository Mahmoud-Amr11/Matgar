namespace Matgar.Api.Requests.Cart
{
    public sealed record AddCartItemRequest(Guid ProductVariantId, int Quantity);

    public sealed record UpdateCartItemRequest(int Quantity);
}
