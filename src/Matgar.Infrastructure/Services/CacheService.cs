using Matgar.Application.Common.Caching;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Matgar.Infrastructure.Services
{
    internal class CacheService(IDistributedCache _cache) : ICacheService
    {
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
        {
            var json = await _cache.GetStringAsync(key, cancellationToken);

            if (string.IsNullOrEmpty(json))
                return default;

            return JsonSerializer.Deserialize<T>(json);

        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? duration = null, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize<T>(value);

            await _cache.SetStringAsync(
                key,
                json,
                 new DistributedCacheEntryOptions
                 {
                     AbsoluteExpirationRelativeToNow =
                    duration ?? TimeSpan.FromMinutes(5)
                 }, cancellationToken);
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }

    }
}
