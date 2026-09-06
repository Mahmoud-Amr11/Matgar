using Matgar.Application.Common.Caching;
using Matgar.Application.Features.Products.Queries.Responses;

namespace Matgar.Application.Features.Products.Queries.GetProductById
{
    public sealed record GetProductByIdQuery(Guid ProductId) : ICacheableQuery<ProductDetailsResponse>
    {
        public string CacheKey => $"GetProductById_{ProductId}";

        public TimeSpan? Expiration => TimeSpan.FromMinutes(1);

        public bool BypassCache => false;
    }
}
