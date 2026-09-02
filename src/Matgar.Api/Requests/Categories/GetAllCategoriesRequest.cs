namespace Matgar.Api.Requests.Categories
{
    public sealed record GetAllCategoriesRequest(
    string? Search,
    int Page = 1,
    int PageSize = 10);
}
