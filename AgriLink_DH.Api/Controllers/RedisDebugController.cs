using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RedisDebugController : ControllerBase
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisService _redisService;
    private readonly ILogger<RedisDebugController> _logger;

    public RedisDebugController(
        IConnectionMultiplexer redis,
        RedisService redisService,
        ILogger<RedisDebugController> logger)
    {
        _redis = redis;
        _redisService = redisService;
        _logger = logger;
    }

    /// <summary>
    /// 🔍 Xem tất cả keys trong Redis (pattern: *)
    /// </summary>
    [HttpGet("keys")]
    public ActionResult<ApiResponse<List<string>>> GetAllKeys([FromQuery] string pattern = "*")
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern)
                .Select(k => k.ToString())
                .OrderBy(k => k)
                .ToList();

            return Ok(ApiResponse<List<string>>.SuccessResponse(keys, $"Tìm thấy {keys.Count} keys"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy Redis keys");
            return StatusCode(500, ApiResponse<List<string>>.ErrorResponse(ex.Message, 500));
        }
    }

    /// <summary>
    /// 🔍 Xem value của 1 key cụ thể
    /// </summary>
    [HttpGet("get/{key}")]
    public async Task<ActionResult<ApiResponse<object>>> GetValue(string key)
    {
        try
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);

            if (!value.HasValue)
            {
                return NotFound(ApiResponse<object>.NotFoundResponse($"Key '{key}' không tồn tại"));
            }

            // Try parse JSON
            object result;
            try
            {
                result = JsonSerializer.Deserialize<object>(value.ToString());
            }
            catch
            {
                result = value.ToString(); // Plain string
            }

            return Ok(ApiResponse<object>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy value từ Redis");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, 500));
        }
    }

    /// <summary>
    /// 🔍 Xem TTL (Time To Live) của key
    /// </summary>
    [HttpGet("ttl/{key}")]
    public async Task<ActionResult<ApiResponse<object>>> GetTTL(string key)
    {
        try
        {
            var db = _redis.GetDatabase();
            var ttl = await db.KeyTimeToLiveAsync(key);

            if (!ttl.HasValue)
            {
                return Ok(ApiResponse<object>.SuccessResponse(new
                {
                    Key = key,
                    TTL = "Không có TTL (lưu vĩnh viễn)",
                    Seconds = -1
                }));
            }

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                Key = key,
                TTL = ttl.Value.ToString(),
                TotalSeconds = (int)ttl.Value.TotalSeconds,
                ExpiresAt = DateTime.UtcNow.Add(ttl.Value)
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy TTL");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, 500));
        }
    }

    /// <summary>
    /// 🗑️ Xóa 1 key
    /// </summary>
    [HttpDelete("delete/{key}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteKey(string key)
    {
        try
        {
            var result = await _redisService.DeleteAsync(key);
            var message = result ? $"Đã xóa key '{key}'" : $"Key '{key}' không tồn tại";
            return Ok(ApiResponse<bool>.SuccessResponse(result, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa key");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(ex.Message, 500));
        }
    }

    /// <summary>
    /// 🔥 DANGER: Xóa TẤT CẢ keys (flush database)
    /// </summary>
    [HttpDelete("flush-all")]
    public ActionResult<ApiResponse<string>> FlushAll()
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            server.FlushDatabase();
            return Ok(ApiResponse<string>.SuccessResponse("Database đã được flush!", "⚠️ Đã xóa TẤT CẢ cache"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi flush database");
            return StatusCode(500, ApiResponse<string>.ErrorResponse(ex.Message, 500));
        }
    }

    /// <summary>
    /// ℹ️ Redis Server Info
    /// </summary>
    [HttpGet("info")]
    public ActionResult<ApiResponse<object>> GetServerInfo()
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var info = server.Info("Server");
            
            var result = new
            {
                Endpoint = server.EndPoint.ToString(),
                IsConnected = server.IsConnected,
                DatabaseCount = server.DatabaseCount,
                Version = info.FirstOrDefault()?.FirstOrDefault(i => i.Key == "redis_version").Value,
                UptimeInSeconds = info.FirstOrDefault()?.FirstOrDefault(i => i.Key == "uptime_in_seconds").Value
            };

            return Ok(ApiResponse<object>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy server info");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, 500));
        }
    }

    /// <summary>
    /// 🔑 Lấy tất cả Refresh Tokens đang lưu trong Redis
    /// </summary>
    [HttpGet("refresh-tokens")]
    public async Task<ActionResult<ApiResponse<object>>> GetAllRefreshTokens()
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var db = _redis.GetDatabase();
            
            // Scan tất cả keys có pattern "refresh_token:*" (với underscore)
            var refreshTokenKeys = server.Keys(pattern: "refresh_token:*")
                .Select(k => k.ToString())
                .ToList();

            var refreshTokens = new List<object>();

            foreach (var key in refreshTokenKeys)
            {
                // Get refresh token value
                var tokenValue = await db.StringGetAsync(key);
                
                // Get TTL
                var ttl = await db.KeyTimeToLiveAsync(key);
                
                // Extract userId from key (format: "refresh_token:userId")
                var userId = key.Replace("refresh_token:", "");

                refreshTokens.Add(new
                {
                    UserId = userId,
                    RefreshToken = tokenValue.ToString(),
                    TTL = ttl.HasValue ? $"{(int)ttl.Value.TotalSeconds} seconds" : "No expiry",
                    ExpiresAt = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : (DateTime?)null
                });
            }

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                TotalTokens = refreshTokens.Count,
                Tokens = refreshTokens
            }, $"Tìm thấy {refreshTokens.Count} refresh tokens"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy refresh tokens");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message, 500));
        }
    }
}
