using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace NiyaCRM.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;
        private readonly IMemoryCache _memoryCache;
        private const string HealthCacheKey = "health_status";
        private const string ReadyCacheKey = "ready_status";
        private const string LiveCacheKey = "live_status";
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromSeconds(30);

        public HealthController(HealthCheckService healthCheckService, IMemoryCache memoryCache)
        {
            _healthCheckService = healthCheckService;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Gets the health status of the application and its dependencies
        /// </summary>
        /// <returns>Health status information</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (!_memoryCache.TryGetValue(HealthCacheKey, out object? cachedResponse))
            {
                var report = await _healthCheckService.CheckHealthAsync();
                
                var response = new
                {
                    Status = report.Status.ToString(),
                    Checks = report.Entries.Select(e => new
                    {
                        Component = e.Key,
                        Status = e.Value.Status.ToString(),
                        Description = e.Value.Description
                    }),
                    TotalDuration = report.TotalDuration
                };

                // Cache the response for 30 seconds
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(CacheExpiration);
                
                _memoryCache.Set(HealthCacheKey, response, cacheEntryOptions);
                cachedResponse = response;
            }

            // Get the status from the cached response
            if (cachedResponse is not null)
            {
                var status = ((dynamic)cachedResponse).Status?.ToString();
                return status == HealthStatus.Healthy.ToString() 
                    ? Ok(cachedResponse) 
                    : StatusCode(503, cachedResponse);
            }
            
            // Fallback if cache somehow returned null
            return StatusCode(500, new { Status = "Error", Message = "Failed to retrieve status" });
        }

        /// <summary>
        /// Gets the readiness status of the application
        /// </summary>
        /// <returns>Readiness status information</returns>
        [HttpGet("ready")]
        public async Task<IActionResult> GetReady()
        {
            if (!_memoryCache.TryGetValue(ReadyCacheKey, out object? cachedResponse))
            {
                var report = await _healthCheckService.CheckHealthAsync(
                    predicate: check => check.Tags.Contains("service"));
                
                var response = new
                {
                    Status = report.Status.ToString(),
                    Checks = report.Entries.Select(e => new
                    {
                        Component = e.Key,
                        Status = e.Value.Status.ToString(),
                        Description = e.Value.Description
                    }),
                    TotalDuration = report.TotalDuration
                };

                // Cache the response for 30 seconds
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(CacheExpiration);
                
                _memoryCache.Set(ReadyCacheKey, response, cacheEntryOptions);
                cachedResponse = response;
            }

            // Get the status from the cached response
            if (cachedResponse is not null)
            {
                var status = ((dynamic)cachedResponse).Status?.ToString();
                return status == HealthStatus.Healthy.ToString() 
                    ? Ok(cachedResponse) 
                    : StatusCode(503, cachedResponse);
            }
            
            // Fallback if cache somehow returned null
            return StatusCode(500, new { Status = "Error", Message = "Failed to retrieve status" });
        }

        /// <summary>
        /// Gets the liveness status of the application
        /// </summary>
        /// <returns>Liveness status information</returns>
        [HttpGet("live")]
        public async Task<IActionResult> GetLive()
        {
            if (!_memoryCache.TryGetValue(LiveCacheKey, out object? cachedResponse))
            {
                var report = await _healthCheckService.CheckHealthAsync();
                
                var response = new
                {
                    Status = report.Status.ToString(),
                    Checks = report.Entries.Select(e => new
                    {
                        Component = e.Key,
                        Status = e.Value.Status.ToString(),
                        Description = e.Value.Description
                    }),
                    TotalDuration = report.TotalDuration
                };

                // Cache the response for 30 seconds
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(CacheExpiration);
                
                _memoryCache.Set(LiveCacheKey, response, cacheEntryOptions);
                cachedResponse = response;
            }

            // Get the status from the cached response
            if (cachedResponse is not null)
            {
                var status = ((dynamic)cachedResponse).Status?.ToString();
                return status == HealthStatus.Healthy.ToString() 
                    ? Ok(cachedResponse) 
                    : StatusCode(503, cachedResponse);
            }
            
            // Fallback if cache somehow returned null
            return StatusCode(500, new { Status = "Error", Message = "Failed to retrieve status" });
        }
    }
}
