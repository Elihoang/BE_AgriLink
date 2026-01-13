using StackExchange.Redis;
using System.Text.Json;

namespace AgriLink_DH.Core.Services;

public class RedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = _redis.GetDatabase();
    }

    // ========== Refresh Token Methods ==========
    
    public async Task<bool> SetRefreshTokenAsync(string userId, string refreshToken, TimeSpan expiration)
    {
        var key = $"refresh_token:{userId}";
        return await _database.StringSetAsync(key, refreshToken, expiration);
    }

    public async Task<string?> GetRefreshTokenAsync(string userId)
    {
        var key = $"refresh_token:{userId}";
        var value = await _database.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task<bool> DeleteRefreshTokenAsync(string userId)
    {
        var key = $"refresh_token:{userId}";
        return await _database.KeyDeleteAsync(key);
    }

    // ========== Generic Cache Methods ==========
    
    /// <summary>
    /// Get cached object (deserialized from JSON)
    /// </summary>
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = await _database.StringGetAsync(key);
        if (!value.HasValue)
            return null;

        return JsonSerializer.Deserialize<T>(value.ToString());
    }

    /// <summary>
    /// Set object to cache (serialized as JSON)
    /// </summary>
    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var json = JsonSerializer.Serialize(value);
        
        if (expiration.HasValue)
        {
            return await _database.StringSetAsync(key, json, expiration.Value);
        }
        
        return await _database.StringSetAsync(key, json);
    }

    /// <summary>
    /// Delete cache key
    /// </summary>
    public async Task<bool> DeleteAsync(string key)
    {
        return await _database.KeyDeleteAsync(key);
    }

    /// <summary>
    /// Check if key exists
    /// </summary>
    public async Task<bool> ExistsAsync(string key)
    {
        return await _database.KeyExistsAsync(key);
    }

    /// <summary>
    /// Delete keys matching a pattern (e.g. "prefix:*")
    /// Warning: Uses KEYS/SCAN, performant impact on large datasets
    /// </summary>
    public async Task DeleteByPatternAsync(string pattern)
    {
        // Get all endpoints
        var endpoints = _redis.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            var server = _redis.GetServer(endpoint);
            if (server.IsConnected)
            {
                // Use Keys() which uses SCAN under the hood in newer StackExchange.Redis
                await foreach (var key in server.KeysAsync(pattern: pattern))
                {
                    await _database.KeyDeleteAsync(key);
                }
            }
        }
    }
}
