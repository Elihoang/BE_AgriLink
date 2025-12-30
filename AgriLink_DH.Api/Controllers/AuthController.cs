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
        // Get IP address and User-Agent
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var (success, message, user, token) = await _authService.RegisterAsync(registerDto, ipAddress, userAgent);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            User = user,
            Token = token
        }, message));
    }

    /// Đăng nhập
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<object>>> Login([FromBody] LoginDto loginDto)
    {
        // Get IP address and User-Agent for login tracking
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var (success, message, user, token) = await _authService.LoginAsync(loginDto, ipAddress, userAgent);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            User = user,
            Token = token
        }, message));
    }

    
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<TokenDto?>>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        var (success, message, token) = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);

        if (!success || token == null)
        {
            return BadRequest(ApiResponse<TokenDto?>.ErrorResponse(message));
        }

        return Ok(ApiResponse<TokenDto?>.SuccessResponse(token, message));
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
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var success = await _authService.LogoutAsync(userId, ipAddress, userAgent);

        if (!success)
        {
            return BadRequest(ApiResponse<object?>.ErrorResponse("Logout failed"));
        }

        return Ok(ApiResponse<object?>.SuccessResponse(null, "Logout successful"));
    }

    
    /// Lấy thông tin user hiện tại (test JWT)
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public ActionResult<ApiResponse<object>> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            UserId = userId,
            Username = username,
            Email = email,
            Role = role
        }, "User information retrieved successfully"));
    }
}
