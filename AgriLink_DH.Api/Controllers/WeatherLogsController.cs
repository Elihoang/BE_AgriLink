using AgriLink_DH.Api.Extensions;
using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.WeatherLog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WeatherLogsController : ControllerBase
{
    private readonly WeatherLogService _weatherLogService;
    private readonly ILogger<WeatherLogsController> _logger;

    public WeatherLogsController(
        WeatherLogService weatherLogService,
        ILogger<WeatherLogsController> logger)
    {
        _weatherLogService = weatherLogService;
        _logger = logger;
    }

    [HttpGet("by-farm/{farmId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WeatherLogDto>>>> GetByFarm(Guid farmId)
    {
        try
        {
            var logs = await _weatherLogService.GetByFarmAsync(farmId);
            return Ok(ApiResponse<IEnumerable<WeatherLogDto>>.SuccessResponse(logs, "Lấy nhật ký thời tiết thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy nhật ký thời tiết");
            return StatusCode(500, ApiResponse<IEnumerable<WeatherLogDto>>.ErrorResponse("Lỗi khi lấy nhật ký thời tiết", 500));
        }
    }

    [HttpGet("my-logs")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WeatherLogDto>>>> GetMyLogs()
    {
        try
        {
            var userId = User.GetUserId();
            var logs = await _weatherLogService.GetByUserIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<WeatherLogDto>>.SuccessResponse(logs, "Lấy nhật ký thời tiết của tôi thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy nhật ký thời tiết của tôi");
            return StatusCode(500, ApiResponse<IEnumerable<WeatherLogDto>>.ErrorResponse("Lỗi khi lấy nhật ký thời tiết", 500));
        }
    }

    [HttpGet("by-date-range")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WeatherLogDto>>>> GetByDateRange(
        [FromQuery] Guid farmId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var logs = await _weatherLogService.GetByDateRangeAsync(farmId, startDate, endDate);
            return Ok(ApiResponse<IEnumerable<WeatherLogDto>>.SuccessResponse(logs, "Lấy nhật ký thời tiết thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy nhật ký thời tiết");
            return StatusCode(500, ApiResponse<IEnumerable<WeatherLogDto>>.ErrorResponse("Lỗi khi lấy nhật ký thời tiết", 500));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<WeatherLogDto>>> Create([FromBody] CreateWeatherLogDto dto)
    {
        try
        {
            var log = await _weatherLogService.CreateLogAsync(dto);
            return Ok(ApiResponse<WeatherLogDto>.CreatedResponse(log, "Tạo nhật ký thời tiết mới thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<WeatherLogDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo nhật ký thời tiết mới");
            return StatusCode(500, ApiResponse<WeatherLogDto>.ErrorResponse("Lỗi khi tạo nhật ký thời tiết mới", 500));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var result = await _weatherLogService.DeleteLogAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Xóa nhật ký thời tiết thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa nhật ký thời tiết");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi xóa nhật ký thời tiết", 500));
        }
    }
}
