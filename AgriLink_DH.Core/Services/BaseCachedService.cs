using AgriLink_DH.Domain.Interface;

namespace AgriLink_DH.Core.Services;

/// <summary>
/// Abstract base class for services that use Redis caching.
/// Implements the Cache-Aside Pattern with graceful degradation —
/// if Redis is unavailable or times out, the call falls back to the
/// data source transparently without propagating exceptions.
/// </summary>
public abstract class BaseCachedService
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

    protected readonly ICacheService CacheService;

    protected BaseCachedService(ICacheService cacheService)
    {
        CacheService = cacheService;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Cache-Read Helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a single object from cache; on miss, fetches from <paramref name="fetchAsync"/>,
    /// stores the result, and returns it. Redis failures are silently swallowed.
    /// </summary>
    protected async Task<T?> GetOrSetCacheAsync<T>(
        string         cacheKey,
        Func<Task<T?>> fetchAsync,
        TimeSpan?      expiration = null) where T : class
    {
        // 1. Try cache first
        try
        {
            var cached = await CacheService.GetAsync<T>(cacheKey);
            if (cached is not null)
                return cached;
        }
        catch
        {
            // Redis unavailable — fall through to data source
        }

        // 2. Cache miss — query data source
        var data = await fetchAsync();
        if (data is null)
            return null;

        // 3. Populate cache (best-effort — never throws)
        try { await CacheService.SetAsync(cacheKey, data, expiration ?? DefaultExpiration); }
        catch { /* ignore */ }

        return data;
    }

    /// <summary>
    /// Returns a collection from cache; on miss, fetches from <paramref name="fetchAsync"/>,
    /// stores the result, and returns it. Redis failures are silently swallowed.
    /// </summary>
    protected async Task<IEnumerable<T>> GetOrSetCacheListAsync<T>(
        string                     cacheKey,
        Func<Task<IEnumerable<T>>> fetchAsync,
        TimeSpan?                  expiration = null)
    {
        // 1. Try cache first
        try
        {
            var cached = await CacheService.GetAsync<List<T>>(cacheKey);
            if (cached is not null)
                return cached;
        }
        catch
        {
            // Redis unavailable — fall through to data source
        }

        // 2. Cache miss — query data source
        var dataList = (await fetchAsync()).ToList();

        // 3. Populate cache (best-effort — never throws)
        try { await CacheService.SetAsync(cacheKey, dataList, expiration ?? DefaultExpiration); }
        catch { /* ignore */ }

        return dataList;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Cache-Invalidation Helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Removes a single cache entry. Silently ignores Redis errors.</summary>
    protected async Task InvalidateCacheAsync(string cacheKey)
    {
        try { await CacheService.DeleteAsync(cacheKey); }
        catch { /* ignore */ }
    }

    /// <summary>Removes multiple cache entries in sequence. Silently ignores Redis errors.</summary>
    protected async Task InvalidateMultipleCachesAsync(params string[] cacheKeys)
    {
        foreach (var key in cacheKeys)
            await InvalidateCacheAsync(key);
    }

    /// <summary>
    /// Removes all cache entries whose keys match <paramref name="pattern"/> (e.g. <c>prefix:*</c>).
    /// Silently ignores Redis errors.
    /// </summary>
    protected async Task InvalidateCacheByPatternAsync(string pattern)
    {
        try { await CacheService.DeleteByPatternAsync(pattern); }
        catch { /* ignore */ }
    }

    /// <summary>Returns <c>true</c> if the cache key exists; <c>false</c> on any Redis error.</summary>
    protected async Task<bool> CacheExistsAsync(string cacheKey)
    {
        try { return await CacheService.ExistsAsync(cacheKey); }
        catch { return false; }
    }
}
