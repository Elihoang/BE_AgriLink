using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.CropSeason;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CropSeasonsController : ControllerBase
{
    private readonly CropSeasonService _cropSeasonService;
    private readonly ILogger<CropSeasonsController> _logger;

    public CropSeasonsController(CropSeasonService cropSeasonService, ILogger<CropSeasonsController> logger)
    {
        _cropSeasonService = cropSeasonService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CropSeasonDto>>>> GetAllSeasons()
    {
        try
        {
            var seasons = await _cropSeasonService.GetAllSeasonsAsync();
            return Ok(ApiResponse<IEnumerable<CropSeasonDto>>.SuccessResponse(seasons, "Lấy danh sách vụ mùa thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách vụ mùa");
            return StatusCode(500, ApiResponse<IEnumerable<CropSeasonDto>>.ErrorResponse("Lỗi khi lấy danh sách vụ mùa", 500));
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CropSeasonDto>>>> GetActiveSeasons()
    {
        try
        {
            var seasons = await _cropSeasonService.GetActiveSeasonsAsync();
            return Ok(ApiResponse<IEnumerable<CropSeasonDto>>.SuccessResponse(seasons, "Lấy danh sách vụ mùa đang hoạt động thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách vụ mùa đang hoạt động");
            return StatusCode(500, ApiResponse<IEnumerable<CropSeasonDto>>.ErrorResponse("Lỗi khi lấy danh sách vụ mùa", 500));
        }
    }

    [HttpGet("by-farm/{farmId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CropSeasonDto>>>> GetSeasonsByFarm(Guid farmId)
    {
        try
        {
            var seasons = await _cropSeasonService.GetSeasonsByFarmIdAsync(farmId);
            return Ok(ApiResponse<IEnumerable<CropSeasonDto>>.SuccessResponse(seasons, "Lấy danh sách vụ mùa theo vườn thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách vụ mùa của vườn {FarmId}", farmId);
            return StatusCode(500, ApiResponse<IEnumerable<CropSeasonDto>>.ErrorResponse("Lỗi khi lấy danh sách vụ mùa", 500));
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CropSeasonDto>>> GetSeasonById(Guid id)
    {
        try
        {
            var season = await _cropSeasonService.GetSeasonByIdAsync(id);
            if (season == null)
            {
                return NotFound(ApiResponse<CropSeasonDto>.NotFoundResponse($"Không tìm thấy vụ mùa với ID: {id}"));
            }

            return Ok(ApiResponse<CropSeasonDto>.SuccessResponse(season, "Lấy thông tin vụ mùa thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin vụ mùa {Id}", id);
            return StatusCode(500, ApiResponse<CropSeasonDto>.ErrorResponse("Lỗi khi lấy thông tin vụ mùa", 500));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CropSeasonDto>>> CreateSeason([FromBody] CreateCropSeasonDto dto)
    {
        try
        {
            var season = await _cropSeasonService.CreateSeasonAsync(dto);
            return CreatedAtAction(nameof(GetSeasonById), new { id = season.Id },
                ApiResponse<CropSeasonDto>.CreatedResponse(season, "Tạo vụ mùa mới thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CropSeasonDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo vụ mùa mới");
            return StatusCode(500, ApiResponse<CropSeasonDto>.ErrorResponse("Lỗi khi tạo vụ mùa mới", 500));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CropSeasonDto>>> UpdateSeason(Guid id, [FromBody] UpdateCropSeasonDto dto)
    {
        try
        {
            var season = await _cropSeasonService.UpdateSeasonAsync(id, dto);
            return Ok(ApiResponse<CropSeasonDto>.SuccessResponse(season, "Cập nhật vụ mùa thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CropSeasonDto>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật vụ mùa {Id}", id);
            return StatusCode(500, ApiResponse<CropSeasonDto>.ErrorResponse("Lỗi khi cập nhật vụ mùa", 500));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSeason(Guid id)
    {
        try
        {
            var result = await _cropSeasonService.DeleteSeasonAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Xóa vụ mùa thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa vụ mùa {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi xóa vụ mùa", 500));
        }
    }
}
