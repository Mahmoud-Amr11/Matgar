namespace Matgar.Application.Features.Cart.Queries.Responses
{
    public sealed record CartResponse(
        Guid CartId,
        Guid? UserId,
        DateTime CreatedAt,
        IReadOnlyList<CartItemResponse> Items,
        decimal TotalPrice);

    public sealed record CartItemResponse(
        Guid ItemId,
        Guid ProductVariantId,
        string Sku,
        string ProductName,
        string? ImageUrl,
        string AttributesJson,
        decimal UnitPrice,
        int Quantity,
        decimal LineTotal);

    public sealed record AddCartItemResponse(
        Guid CartId,
        Guid CartItemId,
        int Quantity);
}
