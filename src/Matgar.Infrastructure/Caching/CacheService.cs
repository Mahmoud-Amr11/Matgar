using Matgar.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace Matgar.Infrastructure.Caching
{
    internal class CacheService(IDistributedCache _cache, IConnectionMultiplexer? _redis = null) : ICacheService
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

        public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            if (_redis is null || !_redis.IsConnected)
                return;

            var database = _redis.GetDatabase();

            foreach (var endpoint in _redis.GetEndPoints())
            {
                var server = _redis.GetServer(endpoint);

                if (!server.IsConnected || server.IsReplica)
                    continue;

                var keys = server.Keys(pattern: $"{prefix}*", pageSize: 250).ToArray();
                if (keys.Length > 0)
                    await database.KeyDeleteAsync(keys);
            }
        }

    }
}
