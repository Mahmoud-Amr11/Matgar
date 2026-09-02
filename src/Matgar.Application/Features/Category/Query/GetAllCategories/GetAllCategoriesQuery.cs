using Matgar.Application.Common.Caching;
using Matgar.Application.Common.Pagination;
using Matgar.Application.Features.Category.Query.Responses;

namespace Matgar.Application.Features.Category.Query.GetAllCategories
{
    public sealed record GetAllCategoriesQuery(string? Search,
    int Page = 1,
    int PageSize = 20
        ) : ICacheableQuery<PagedResult<CategoryResponse>>
    {
        public string CacheKey => $"GetAllCategories_Page_{Page}_Size_{PageSize}";
        public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
        public bool BypassCache => false;
    }
}
