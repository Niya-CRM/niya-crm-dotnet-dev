using System;
using System.Threading.Tasks;

namespace NiyaCRM.Core.Cache
{
    /// <summary>
    /// Provides methods for direct interaction with the underlying cache store (e.g., MemoryCache).
    /// </summary>
    public interface ICacheRepository
    {
        /// <summary>
        /// Retrieves a cached item by its key from the underlying cache provider.
        /// </summary>
        /// <typeparam name="T">The type of the cached item.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <returns>The cached item, or null if not found.</returns>
        Task<T?> GetAsync<T>(string key);
        /// <summary>
        /// Sets a cache entry with the specified key and value in the underlying cache provider.
        /// </summary>
        /// <typeparam name="T">The type of the value to cache.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to cache.</param>
        /// <param name="absoluteExpiration">The absolute expiration time, or null to use the default.</param>
        /// <param name="slidingExpiration">The sliding expiration time, or null to use the default.</param>
        Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null);
        /// <summary>
        /// Removes a cache entry by its key from the underlying cache provider.
        /// </summary>
        /// <param name="key">The cache key.</param>
        Task RemoveAsync(string key);
    }
}
