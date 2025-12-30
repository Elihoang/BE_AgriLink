using Microsoft.AspNetCore.Http;

namespace AgriLink_DH.Core.Helpers;


/// Helper để lấy thông tin HTTP request (IP, User-Agent, ...)

public class HttpContextHelper
{
    
    /// Lấy IP Address của client
    /// Xử lý cả trường hợp behind proxy (Nginx, Cloudflare, ...)
    
    public static string GetClientIpAddress(HttpContext context)
    {
        // 1. Kiểm tra X-Forwarded-For header (khi behind proxy/load balancer)
        if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
            
            // X-Forwarded-For có thể chứa nhiều IP (client, proxy1, proxy2...)
            // Lấy IP đầu tiên = IP client thật
            var ips = forwardedFor.Split(',');
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        // 2. Kiểm tra X-Real-IP header (Nginx thường dùng)
        if (context.Request.Headers.ContainsKey("X-Real-IP"))
        {
            return context.Request.Headers["X-Real-IP"].ToString();
        }

        // 3. Fallback: Lấy từ RemoteIpAddress
        var remoteIp = context.Connection.RemoteIpAddress?.ToString();
        
        // Xử lý IPv6 loopback thành IPv4
        if (remoteIp == "::1")
        {
            return "127.0.0.1";
        }

        return remoteIp ?? "Unknown";
    }

    
    /// Lấy User-Agent (thông tin trình duyệt và thiết bị)
    
    public static string GetUserAgent(HttpContext context)
    {
        return context.Request.Headers["User-Agent"].ToString() ?? "Unknown";
    }

    
    /// Parse User-Agent để lấy thông tin device đơn giản
    /// Trả về: "Mobile - Android" hoặc "Desktop - Windows" ...
    
    public static string GetDeviceType(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return "Unknown";

        userAgent = userAgent.ToLower();

        // Mobile devices
        if (userAgent.Contains("iphone"))
            return "Mobile - iPhone";
        if (userAgent.Contains("android") && userAgent.Contains("mobile"))
            return "Mobile - Android";
        if (userAgent.Contains("ipad"))
            return "Tablet - iPad";
        if (userAgent.Contains("android"))
            return "Tablet - Android";

        // Desktop OS
        if (userAgent.Contains("windows"))
            return "Desktop - Windows";
        if (userAgent.Contains("mac os"))
            return "Desktop - macOS";
        if (userAgent.Contains("linux"))
            return "Desktop - Linux";

        return "Unknown Device";
    }

    
    /// Lấy thông tin browser
    
    public static string GetBrowser(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return "Unknown";

        userAgent = userAgent.ToLower();

        if (userAgent.Contains("edg"))
            return "Microsoft Edge";
        if (userAgent.Contains("chrome"))
            return "Google Chrome";
        if (userAgent.Contains("firefox"))
            return "Mozilla Firefox";
        if (userAgent.Contains("safari") && !userAgent.Contains("chrome"))
            return "Safari";
        if (userAgent.Contains("opera") || userAgent.Contains("opr"))
            return "Opera";

        return "Other Browser";
    }
}
