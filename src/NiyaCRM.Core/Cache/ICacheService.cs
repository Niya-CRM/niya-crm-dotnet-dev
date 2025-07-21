using System;
using System.Threading.Tasks;

namespace NiyaCRM.Core.Cache
{
    /// <summary>
    /// Provides methods for interacting with the application cache.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Retrieves a cached item by its key.
        /// </summary>
        /// <typeparam name="T">The type of the cached item.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <returns>The cached item, or null if not found.</returns>
        Task<T?> GetAsync<T>(string key);
        /// <summary>
        /// Sets a cache entry with the specified key and value.
        /// </summary>
        /// <typeparam name="T">The type of the value to cache.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to cache.</param>
        /// <param name="absoluteExpiration">The absolute expiration time, or null to use the default.</param>
        /// <param name="slidingExpiration">The sliding expiration time, or null to use the default.</param>
        Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null);
        /// <summary>
        /// Removes a cache entry by its key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        Task RemoveAsync(string key);
    }
}
