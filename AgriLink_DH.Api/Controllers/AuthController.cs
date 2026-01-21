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
        // Get IP address and User-Agent
        var ipAddress = IpAddressHelper.GetClientIpAddress(HttpContext);
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
        var ipAddress = IpAddressHelper.GetClientIpAddress(HttpContext);
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
        var ipAddress = IpAddressHelper.GetClientIpAddress(HttpContext);
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var success = await _authService.LogoutAsync(userId, ipAddress, userAgent);

        if (!success)
        {
            return BadRequest(ApiResponse<object?>.ErrorResponse("Logout failed"));
        }

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
