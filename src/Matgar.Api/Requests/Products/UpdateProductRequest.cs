namespace Matgar.Api.Requests.Products
{
    public sealed record UpdateProductRequest(string Name, string Description, Guid CategoryId);

}
