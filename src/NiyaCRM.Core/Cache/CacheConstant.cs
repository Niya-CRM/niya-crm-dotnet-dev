namespace NiyaCRM.Core.Cache
{
    public static class CacheConstant
    {
        public static readonly TimeSpan DEFAULT_CACHE_EXPIRATION = TimeSpan.FromMinutes(180);
        public static readonly TimeSpan DEFAULT_CACHE_SLIDING_EXPIRATION = TimeSpan.FromMinutes(30);
    }
}
