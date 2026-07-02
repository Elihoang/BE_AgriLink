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
        var users = await _userService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<UserDto>>.SuccessResponse(users));
    }

    /// <summary>
    /// Lấy thông tin user theo ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound(ApiResponse<UserDto>.NotFoundResponse($"Không tìm thấy người dùng với ID: {id}"));
        }

        return Ok(ApiResponse<UserDto>.SuccessResponse(user));
    }

    /// <summary>
    /// Tạo user mới (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create([FromBody] CreateUserDto dto)
    {
        var user = await _userService.CreateUserAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = user.Id },
            ApiResponse<UserDto>.CreatedResponse(user, "Tạo người dùng thành công"));
    }

    /// <summary>
    /// Cập nhật thông tin user (partial update)
    /// </summary>
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        var user = await _userService.UpdateUserAsync(id, dto);
        return Ok(ApiResponse<UserDto>.SuccessResponse(user, "Cập nhật thông tin thành công"));
    }

    /// <summary>
    /// Đổi mật khẩu
    /// </summary>
    [HttpPut("{id:guid}/change-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword(Guid id, [FromBody] ChangePasswordDto dto)
    {
        var result = await _userService.ChangePasswordAsync(id, dto.CurrentPassword, dto.NewPassword);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Đổi mật khẩu thành công"));
    }

    /// <summary>
    /// Kích hoạt/Vô hiệu hóa user (Admin only)
    /// </summary>
    [HttpPut("{id:guid}/toggle-status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleStatus(Guid id, [FromBody] ToggleStatusDto dto)
    {
        var result = await _userService.ToggleActiveStatusAsync(id, dto.IsActive);
        var message = dto.IsActive ? "Kích hoạt tài khoản thành công" : "Vô hiệu hóa tài khoản thành công";
        return Ok(ApiResponse<bool>.SuccessResponse(result, message));
    }

    /// <summary>
    /// Lấy lịch sử đăng nhập
    /// </summary>
    [HttpGet("{id:guid}/login-history")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserLoginLogDto>>>> GetLoginHistory(Guid id, [FromQuery] int count = 10)
    {
        var logs = await _userService.GetLoginHistoryAsync(id, count);
        return Ok(ApiResponse<IEnumerable<UserLoginLogDto>>.SuccessResponse(logs));
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
