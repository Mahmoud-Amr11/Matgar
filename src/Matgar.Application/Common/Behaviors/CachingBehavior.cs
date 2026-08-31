using Matgar.Application.Common.Caching;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Common.Behaviors
{
    public class CachingBehavior<TRequest, TValue>
        : IPipelineBehavior<
            TRequest,
            Result<TValue>>
    where TRequest : ICacheableQuery<TValue>
    {
        private readonly ICacheService _cache;

        public CachingBehavior(ICacheService cache)
        {
            _cache = cache;
        }

        public async Task<Result<TValue>> Handle(TRequest request, RequestHandlerDelegate<Result<TValue>> next, CancellationToken cancellationToken)
        {

            if (request.BypassCache)
                return await next();

            var cached = await _cache.GetAsync<TValue>(request.CacheKey);

            if (cached is not null)
                return Result<TValue>.Success(cached);

            var response = await next();

            if (response.IsSuccess)
            {
                await _cache.SetAsync(
                    request.CacheKey,
                    response.Value,
                    request.Expiration);
            }

            return response;
        }
    }
}
