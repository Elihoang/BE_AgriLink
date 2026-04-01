using AgriLink_DH.Core.Helpers;
using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// Đăng ký tài khoản mới
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<object>>> Register([FromBody] RegisterDto registerDto)
    {
        var ipAddress = IpAddressHelper.GetClientIpAddress(HttpContext);
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var (success, message, user, token) = await _authService.RegisterAsync(registerDto, ipAddress, userAgent);

        if (!success) return BadRequest(ApiResponse<object>.ErrorResponse(message));

        if (token != null)
        {
            SetRefreshTokenCookie(token.RefreshToken);
            token.RefreshToken = string.Empty;
        }

        return Ok(ApiResponse<object>.SuccessResponse(new { User = user, Token = token }, message));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<object>>> Login([FromBody] LoginDto loginDto)
    {
        var ipAddress = IpAddressHelper.GetClientIpAddress(HttpContext);
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var (success, message, user, token) = await _authService.LoginAsync(loginDto, ipAddress, userAgent);

        if (!success) return BadRequest(ApiResponse<object>.ErrorResponse(message));

        if (token != null)
        {
            SetRefreshTokenCookie(token.RefreshToken);
            token.RefreshToken = string.Empty;
        }

        return Ok(ApiResponse<object>.SuccessResponse(new { User = user, Token = token }, message));
    }

    
    /// <summary>
    /// Refresh access token using HttpOnly cookie
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<TokenDto?>>> RefreshToken()
    {
        // 1. Lấy AccessToken cũ từ Header (do đã có [Authorize])
        var accessToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        
        // 2. Lấy RefreshToken từ HttpOnly Cookie
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
        {
             return Unauthorized(ApiResponse<TokenDto?>.ErrorResponse("Refresh Session đã hết hạn. Vui lòng đăng nhập lại.", 401));
        }

        var (success, message, token) = await _authService.RefreshTokenAsync(accessToken, refreshToken);

        if (!success || token == null)
        {
            return BadRequest(ApiResponse<TokenDto?>.ErrorResponse(message));
        }

        // 3. Ghi lại cookie và ẩn refreshToken khỏi JSON
        SetRefreshTokenCookie(token.RefreshToken);
        token.RefreshToken = string.Empty; 

        return Ok(ApiResponse<TokenDto?>.SuccessResponse(token, message));
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Đổi thành false để test trên HTTP localhost
            SameSite = SameSiteMode.Lax, // Đổi thành Lax để browser chấp nhận trên cùng localhost
            Expires = DateTime.UtcNow.AddDays(7),
            Path = "/"
        };
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    private void RemoveRefreshTokenCookie()
    {
        Response.Cookies.Delete("refreshToken");
    }

    /// Đăng xuất - xóa refresh token
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object?>>> Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<object?>.ErrorResponse("User not authenticated", 401));
        }

        // Get IP and User-Agent
        var ipAddress = IpAddressHelper.GetClientIpAddress(HttpContext);
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var success = await _authService.LogoutAsync(userId, ipAddress, userAgent);

        if (!success)
        {
            return BadRequest(ApiResponse<object?>.ErrorResponse("Logout failed"));
        }

        RemoveRefreshTokenCookie();
        return Ok(ApiResponse<object?>.SuccessResponse(null, "Logout successful"));
    }

    
    /// <summary>
    /// Lấy thông tin user hiện tại (đầy đủ từ database)
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<object>>> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated", 401));
        }

        // Lấy thông tin đầy đủ từ database
        var userDto = await _authService.GetUserByIdAsync(userGuid);
        
        if (userDto == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found", 404));
        }

        return Ok(ApiResponse<object>.SuccessResponse(userDto, "User information retrieved successfully"));
    }
}
