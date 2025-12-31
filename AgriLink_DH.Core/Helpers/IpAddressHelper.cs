using Microsoft.AspNetCore.Http;

namespace AgriLink_DH.Core.Helpers;

public static class IpAddressHelper
{
    /// <summary>
    /// Lấy địa chỉ IP thực của client, xử lý cả trường hợp có reverse proxy
    /// </summary>
    public static string GetClientIpAddress(HttpContext httpContext)
    {
        // Thứ tự ưu tiên:
        // 1. X-Forwarded-For (khi đằng sau reverse proxy như nginx, cloudflare)
        // 2. X-Real-IP (khi đằng sau một số reverse proxy)
        // 3. RemoteIpAddress (khi truy cập trực tiếp)
        
        // Kiểm tra X-Forwarded-For header
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For có thể chứa nhiều IP (client, proxy1, proxy2,...)
            // IP đầu tiên là IP thực của client
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                var clientIp = ips[0].Trim();
                if (!string.IsNullOrEmpty(clientIp) && clientIp != "::1" && clientIp != "127.0.0.1")
                {
                    return clientIp;
                }
            }
        }

        // Kiểm tra X-Real-IP header
        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp) && realIp != "::1" && realIp != "127.0.0.1")
        {
            return realIp;
        }

        // Fallback: Sử dụng RemoteIpAddress
        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();
        
        // Nếu là IPv6 localhost, chuyển sang IPv4
        if (remoteIp == "::1")
        {
            return "127.0.0.1";
        }

        return remoteIp ?? "Unknown";
    }
}
