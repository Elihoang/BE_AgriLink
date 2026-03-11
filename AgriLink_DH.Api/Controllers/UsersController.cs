using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy toàn bộ danh sách người dùng (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserDto>>>> GetAll()
    {
        try
        {
            var users = await _userService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResponse(users));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách người dùng");
            return StatusCode(500, ApiResponse<IEnumerable<UserDto>>.ErrorResponse("Lỗi khi lấy danh sách người dùng", 500));
        }
    }

    /// <summary>
    /// Lấy thông tin user theo ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(Guid id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.NotFoundResponse($"Không tìm thấy người dùng với ID: {id}"));
            }

            return Ok(ApiResponse<UserDto>.SuccessResponse(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin người dùng {Id}", id);
            return StatusCode(500, ApiResponse<UserDto>.ErrorResponse("Lỗi khi lấy thông tin người dùng", 500));
        }
    }

    /// <summary>
    /// Tạo user mới (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create([FromBody] CreateUserDto dto)
    {
        try
        {
            var user = await _userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = user.Id },
                ApiResponse<UserDto>.CreatedResponse(user, "Tạo người dùng thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UserDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo người dùng");
            return StatusCode(500, ApiResponse<UserDto>.ErrorResponse("Lỗi khi tạo người dùng", 500));
        }
    }

    /// <summary>
    /// Cập nhật thông tin user
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(id, dto);
            return Ok(ApiResponse<UserDto>.SuccessResponse(user, "Cập nhật thông tin thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserDto>.NotFoundResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UserDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật người dùng {Id}", id);
            return StatusCode(500, ApiResponse<UserDto>.ErrorResponse("Lỗi khi cập nhật người dùng", 500));
        }
    }

    /// <summary>
    /// Đổi mật khẩu
    /// </summary>
    [HttpPut("{id:guid}/change-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword(Guid id, [FromBody] ChangePasswordDto dto)
    {
        try
        {
            var result = await _userService.ChangePasswordAsync(id, dto.CurrentPassword, dto.NewPassword);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Đổi mật khẩu thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đổi mật khẩu {UserId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi đổi mật khẩu", 500));
        }
    }

    /// <summary>
    /// Kích hoạt/Vô hiệu hóa user (Admin only)
    /// </summary>
    [HttpPut("{id:guid}/toggle-status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleStatus(Guid id, [FromBody] ToggleStatusDto dto)
    {
        try
        {
            var result = await _userService.ToggleActiveStatusAsync(id, dto.IsActive);
            var message = dto.IsActive ? "Kích hoạt tài khoản thành công" : "Vô hiệu hóa tài khoản thành công";
            return Ok(ApiResponse<bool>.SuccessResponse(result, message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thay đổi trạng thái {UserId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi thay đổi trạng thái", 500));
        }
    }

    /// <summary>
    /// Lấy lịch sử đăng nhập
    /// </summary>
    [HttpGet("{id:guid}/login-history")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserLoginLogDto>>>> GetLoginHistory(Guid id, [FromQuery] int count = 10)
    {
        try
        {
            var logs = await _userService.GetLoginHistoryAsync(id, count);
            return Ok(ApiResponse<IEnumerable<UserLoginLogDto>>.SuccessResponse(logs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy lịch sử đăng nhập {UserId}", id);
            return StatusCode(500, ApiResponse<IEnumerable<UserLoginLogDto>>.ErrorResponse("Lỗi khi lấy lịch sử đăng nhập", 500));
        }
    }
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ToggleStatusDto
{
    public bool IsActive { get; set; }
}
