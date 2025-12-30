using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.Farm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FarmsController : ControllerBase
{
    private readonly FarmService _farmService;
    private readonly ILogger<FarmsController> _logger;

    public FarmsController(FarmService farmService, ILogger<FarmsController> logger)
    {
        _farmService = farmService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<FarmDto>>>> GetAllFarms()
    {
        try
        {
            var farms = await _farmService.GetAllFarmsAsync();
            return Ok(ApiResponse<IEnumerable<FarmDto>>.SuccessResponse(farms, "Lấy danh sách vườn thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách vườn");
            return StatusCode(500, ApiResponse<IEnumerable<FarmDto>>.ErrorResponse("Lỗi khi lấy danh sách vườn", 500));
        }
    }

    [HttpGet("my-farms")]
    public async Task<ActionResult<ApiResponse<IEnumerable<FarmDto>>>> GetMyFarms()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<IEnumerable<FarmDto>>.ErrorResponse("Chưa đăng nhập", 401));
            }

            var farms = await _farmService.GetFarmsByUserIdAsync(Guid.Parse(userId));
            return Ok(ApiResponse<IEnumerable<FarmDto>>.SuccessResponse(farms, "Lấy danh sách vườn của bạn thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách vườn của user");
            return StatusCode(500, ApiResponse<IEnumerable<FarmDto>>.ErrorResponse("Lỗi khi lấy danh sách vườn", 500));
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<FarmDto>>> GetFarmById(Guid id)
    {
        try
        {
            var farm = await _farmService.GetFarmByIdAsync(id);
            if (farm == null)
            {
                return NotFound(ApiResponse<FarmDto>.NotFoundResponse($"Không tìm thấy vườn với ID: {id}"));
            }

            return Ok(ApiResponse<FarmDto>.SuccessResponse(farm, "Lấy thông tin vườn thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin vườn {Id}", id);
            return StatusCode(500, ApiResponse<FarmDto>.ErrorResponse("Lỗi khi lấy thông tin vườn", 500));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<FarmDto>>> CreateFarm([FromBody] CreateFarmDto dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<FarmDto>.ErrorResponse("Chưa đăng nhập", 401));
            }

            var farm = await _farmService.CreateFarmAsync(dto, Guid.Parse(userId));
            return CreatedAtAction(nameof(GetFarmById), new { id = farm.Id }, 
                ApiResponse<FarmDto>.CreatedResponse(farm, "Tạo vườn mới thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<FarmDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo vườn mới");
            return StatusCode(500, ApiResponse<FarmDto>.ErrorResponse("Lỗi khi tạo vườn mới", 500));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<FarmDto>>> UpdateFarm(Guid id, [FromBody] UpdateFarmDto dto)
    {
        try
        {
            var farm = await _farmService.UpdateFarmAsync(id, dto);
            return Ok(ApiResponse<FarmDto>.SuccessResponse(farm, "Cập nhật vườn thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<FarmDto>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật vườn {Id}", id);
            return StatusCode(500, ApiResponse<FarmDto>.ErrorResponse("Lỗi khi cập nhật vườn", 500));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteFarm(Guid id)
    {
        try
        {
            var result = await _farmService.DeleteFarmAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Xóa vườn thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa vườn {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi xóa vườn", 500));
        }
    }
}
