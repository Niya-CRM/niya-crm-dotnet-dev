using System;
using System.Threading.Tasks;
using NiyaCRM.Core.Cache;
using Microsoft.Extensions.Logging;

namespace NiyaCRM.Application.Cache
{
    /// <summary>
    /// Implements caching logic and delegates to the repository.
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly ICacheRepository _cacheRepository;
        private readonly Microsoft.Extensions.Logging.ILogger<CacheService> _logger;

        public CacheService(ICacheRepository cacheRepository, Microsoft.Extensions.Logging.ILogger<CacheService> logger)
        {
            _cacheRepository = cacheRepository;
            _logger = logger;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            var sanitizedKey = SanitizeKey(key);
            _logger.LogDebug("Getting cache entry for key: {Key} (sanitized: {SanitizedKey})", key, sanitizedKey);

            return _cacheRepository.GetAsync<T>(sanitizedKey);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            var sanitizedKey = SanitizeKey(key);
            _logger.LogDebug("Setting cache entry for key: {Key} (sanitized: {SanitizedKey}), Expiration: {AbsoluteExpiration}, Sliding: {SlidingExpiration}", key, sanitizedKey, absoluteExpiration, slidingExpiration);

            return _cacheRepository.SetAsync(sanitizedKey, value, absoluteExpiration, slidingExpiration);
        }

        public Task RemoveAsync(string key)
        {
            var sanitizedKey = SanitizeKey(key);
            _logger.LogDebug("Removing cache entry for key: {Key} (sanitized: {SanitizedKey})", key, sanitizedKey);

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
            
            return key.Trim().Replace(" ", "_").ToLowerInvariant();
        }
    }
}
