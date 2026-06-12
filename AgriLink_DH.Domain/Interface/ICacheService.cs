namespace AgriLink_DH.Domain.Interface;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task<bool> DeleteAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task DeleteByPatternAsync(string pattern);
    
    // Refresh token specific - or we can just use generic Set/Get, but let's keep it for compatibility
    Task<bool> SetRefreshTokenAsync(string userId, string refreshToken, TimeSpan expiration);
    Task<string?> GetRefreshTokenAsync(string userId);
    Task<bool> DeleteRefreshTokenAsync(string userId);
}
