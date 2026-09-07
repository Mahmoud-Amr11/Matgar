using Matgar.Application.Common.Caching;
using Matgar.Application.Features.Products.Queries.Responses;

namespace Matgar.Application.Features.Products.Queries.GetProductById
{
    public sealed record GetProductByIdQuery(
        Guid ProductId,
        Guid? RequestingUserId,
        bool IsAdmin) : ICacheableQuery<ProductDetailsResponse>
    {

        public string CacheKey => $"GetProductById_{ProductId}_Viewer_{RequestingUserId}_{IsAdmin}";

        public TimeSpan? Expiration => TimeSpan.FromMinutes(1);

        public bool BypassCache => RequestingUserId is not null || IsAdmin;
    }
}
