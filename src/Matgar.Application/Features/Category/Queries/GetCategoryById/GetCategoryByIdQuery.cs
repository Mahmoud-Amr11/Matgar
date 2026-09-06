using Matgar.Application.Common.Caching;
using Matgar.Application.Features.Category.Query.Responses;

namespace Matgar.Application.Features.Category.Query.GetCategoryById
{
    public sealed record GetCategoryByIdQuery(Guid Id) : ICacheableQuery<CategoryResponse>
    {

        public string CacheKey => $"category-id:{Id}";

        public TimeSpan? Expiration => TimeSpan.FromHours(1);


        public bool BypassCache => false;
    }
}
