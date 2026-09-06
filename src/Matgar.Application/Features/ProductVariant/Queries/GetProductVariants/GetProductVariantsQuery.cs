using Matgar.Application.Common.Caching;
using Matgar.Application.Features.Products.Queries.Responses;

namespace Matgar.Application.Features.ProductVariant.Queries.GetProductVariants
{
    public sealed record GetProductVariantsQuery(Guid ProductId)
       : ICacheableQuery<IReadOnlyList<ProductVariantResponse>>
    {
        public string CacheKey => $"GetProductVariants_{ProductId}";


        public TimeSpan? Expiration => TimeSpan.FromMinutes(1);

        public bool BypassCache => false;
    }
}
