using System;
using System.Threading.Tasks;
using NiyaCRM.Core.Cache;

namespace NiyaCRM.Application.Cache
{
    /// <summary>
    /// Implements caching logic and delegates to the repository.
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly ICacheRepository _cacheRepository;

        public CacheService(ICacheRepository cacheRepository)
        {
            _cacheRepository = cacheRepository;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            var sanitizedKey = SanitizeKey(key);
            return _cacheRepository.GetAsync<T>(sanitizedKey);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            var sanitizedKey = SanitizeKey(key);
            return _cacheRepository.SetAsync(sanitizedKey, value, absoluteExpiration, slidingExpiration);
        }

        public Task RemoveAsync(string key)
        {
            var sanitizedKey = SanitizeKey(key);
            return _cacheRepository.RemoveAsync(sanitizedKey);
        }

        /// <summary>
        /// Sanitizes the cache key to ensure it is valid and consistent.
        /// </summary>
        /// <param name="key">The original cache key.</param>
        /// <returns>A sanitized cache key.</returns>
        private static string SanitizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Cache key cannot be null or whitespace.");
            // Example: trim and replace spaces with underscores
            return key.Trim().Replace(" ", "_").ToLowerInvariant();
        }
    }
}
