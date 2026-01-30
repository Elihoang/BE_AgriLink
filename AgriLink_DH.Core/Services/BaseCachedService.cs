namespace AgriLink_DH.Core.Services;

/// <summary>
/// Base class cho các services có sử dụng Redis caching
/// Cung cấp helper methods chung cho cache-aside pattern
/// </summary>
public abstract class BaseCachedService
{
    protected readonly RedisService RedisService;

    protected BaseCachedService(RedisService redisService)
    {
        RedisService = redisService;
    }

    /// <summary>
    /// Cache-Aside Pattern: Get from cache or fetch from source
    /// </summary>
    /// <typeparam name="T">Type of data to cache</typeparam>
    /// <param name="cacheKey">Redis cache key</param>
    /// <param name="fetchDataFunc">Function to fetch data if cache miss</param>
    /// <param name="expiration">Cache expiration time (optional)</param>
    /// <returns>Cached or fresh data</returns>
    protected async Task<T?> GetOrSetCacheAsync<T>(
        string cacheKey,
        Func<Task<T?>> fetchDataFunc,
        TimeSpan? expiration = null) where T : class
    {
        // 1. Try get from cache
        var cachedData = await RedisService.GetAsync<T>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }

        // 2. Cache miss - fetch from source
        var freshData = await fetchDataFunc();
        if (freshData == null)
        {
            return null;
        }

        // 3. Save to cache
        await RedisService.SetAsync(cacheKey, freshData, expiration);

        return freshData;
    }

    /// <summary>
    /// Cache-Aside Pattern for Lists: Get from cache or fetch from source
    /// </summary>
    protected async Task<IEnumerable<T>> GetOrSetCacheListAsync<T>(
        string cacheKey,
        Func<Task<IEnumerable<T>>> fetchDataFunc,
        TimeSpan? expiration = null)
    {
        // Try get from cache
        var cachedData = await RedisService.GetAsync<List<T>>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }

        // Cache miss - fetch from source
        var freshData = await fetchDataFunc();
        var dataList = freshData.ToList();

        // Save to cache
        await RedisService.SetAsync(cacheKey, dataList, expiration);

        return dataList;
    }

    /// <summary>
    /// Invalidate (delete) a single cache key
    /// </summary>
    protected async Task InvalidateCacheAsync(string cacheKey)
    {
        await RedisService.DeleteAsync(cacheKey);
    }

    /// <summary>
    /// Invalidate multiple cache keys
    /// </summary>
    protected async Task InvalidateMultipleCachesAsync(params string[] cacheKeys)
    {
        foreach (var key in cacheKeys)
        {
            await RedisService.DeleteAsync(key);
        }
    }

    /// <summary>
    /// Invalidate cache keys matching a pattern (e.g., "prefix:*")
    /// </summary>
    protected async Task InvalidateCacheByPatternAsync(string pattern)
    {
        await RedisService.DeleteByPatternAsync(pattern);
    }

    /// <summary>
    /// Check if cache key exists
    /// </summary>
    protected async Task<bool> CacheExistsAsync(string cacheKey)
    {
        return await RedisService.ExistsAsync(cacheKey);
    }
}
