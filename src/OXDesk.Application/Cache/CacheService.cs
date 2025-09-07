using System;
using System.Threading.Tasks;
using OXDesk.Core.Cache;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using OXDesk.Core.Tenants;
namespace OXDesk.Application.Cache
{
    /// <summary>
    /// Implements caching logic and delegates to the repository.
    /// </summary>
    public partial class CacheService : ICacheService
    {
        private readonly ICacheRepository _cacheRepository;
        private readonly ILogger<CacheService> _logger;
        private readonly ICurrentTenant _currentTenant;

        public CacheService(ICacheRepository cacheRepository, ILogger<CacheService> logger, ICurrentTenant currentTenant)
        {
            _cacheRepository = cacheRepository;
            _logger = logger;
            _currentTenant = currentTenant;
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

            absoluteExpiration ??= CacheConstant.DEFAULT_CACHE_EXPIRATION;
            slidingExpiration ??= CacheConstant.DEFAULT_CACHE_SLIDING_EXPIRATION;

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
        private string SanitizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Cache key cannot be null or whitespace.");

            var trimmed = key.Trim().ToLowerInvariant();

            var tenantPart = _currentTenant?.Id.HasValue == true
                ? _currentTenant.Id!.Value.ToString("N")
                : "global";

            var combined = $"t:{tenantPart}:{trimmed}";
            return MyRegex().Replace(combined, "");
        }

        [GeneratedRegex("[^a-zA-Z0-9_:]")]
        private static partial Regex MyRegex();
    }
}
