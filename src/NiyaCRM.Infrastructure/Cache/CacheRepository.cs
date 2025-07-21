using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using NiyaCRM.Core.Cache;

namespace NiyaCRM.Infrastructure.Cache
{
    /// <summary>
    /// MemoryCache-based implementation of ICacheRepository.
    /// </summary>
    public class CacheRepository : ICacheRepository
    {
        private readonly IMemoryCache _memoryCache;

        public CacheRepository(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            if (_memoryCache.TryGetValue(key, out var value) && value is T typedValue)
                return Task.FromResult<T?>(typedValue);
            return Task.FromResult<T?>(default);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            var options = new MemoryCacheEntryOptions();
            if (absoluteExpiration.HasValue)
                options.SetAbsoluteExpiration(absoluteExpiration.Value);
            if (slidingExpiration.HasValue)
                options.SetSlidingExpiration(slidingExpiration.Value);
            _memoryCache.Set(key, value, options);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }
    }
}
