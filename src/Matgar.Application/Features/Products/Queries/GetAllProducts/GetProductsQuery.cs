using Matgar.Application.Common.Caching;
using Matgar.Application.Common.Pagination;
using Matgar.Application.Features.Products.Queries.Responses;

namespace Matgar.Application.Features.Products.Queries.GetAllProducts
{
    public sealed record GetProductsQuery(
       string? Search,
       Guid? CategoryId,
       decimal? MinPrice,
       decimal? MaxPrice,
       ProductStatus? Status,
       int Page = 1,
       int PageSize = 20
   ) : ICacheableQuery<PagedResult<ProductListItemResponse>>
    {
        public string CacheKey
        {
            get
            {
                var normalizedSearch = string.IsNullOrWhiteSpace(Search)
                    ? "all"
                    : Search.Trim().ToLowerInvariant();

                return
                    $"GetProducts_Search_{normalizedSearch}_Cat_{CategoryId}_Min_{MinPrice}_Max_{MaxPrice}_Status_{Status}_Page_{Page}_Size_{PageSize}";
            }
        }
        public TimeSpan? Expiration => TimeSpan.FromMinutes(2);

        public bool BypassCache => false;
    }
}
