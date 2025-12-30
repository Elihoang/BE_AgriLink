using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.PlantPosition;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlantPositionsController : ControllerBase
{
    private readonly PlantPositionService _plantPositionService;
    private readonly ILogger<PlantPositionsController> _logger;

    public PlantPositionsController(
        PlantPositionService plantPositionService,
        ILogger<PlantPositionsController> logger)
    {
        _plantPositionService = plantPositionService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy sơ đồ vị trí cây của 1 rẫy/vụ (CÓ CACHE - NHANH)
    /// </summary>
    [HttpGet("season/{seasonId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlantPositionDto>>>> GetBySeason(Guid seasonId)
    {
        try
        {
            var positions = await _plantPositionService.GetBySeasonAsync(seasonId);
            return Ok(ApiResponse<IEnumerable<PlantPositionDto>>.SuccessResponse(positions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy sơ đồ cây {SeasonId}", seasonId);
            return StatusCode(500, ApiResponse<IEnumerable<PlantPositionDto>>.ErrorResponse("Lỗi khi lấy sơ đồ cây", 500));
        }
    }

    /// <summary>
    /// Tổng quan số lượng từng loại cây
    /// </summary>
    [HttpGet("season/{seasonId:guid}/summary")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, int>>>> GetSummary(Guid seasonId)
    {
        try
        {
            var summary = await _plantPositionService.GetPlantSummaryAsync(seasonId);
            return Ok(ApiResponse<Dictionary<string, int>>.SuccessResponse(summary));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy tổng quan {SeasonId}", seasonId);
            return StatusCode(500, ApiResponse<Dictionary<string, int>>.ErrorResponse("Lỗi khi lấy tổng quan", 500));
        }
    }

    /// <summary>
    /// Thêm 1 cây vào vị trí cụ thể
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PlantPositionDto>>> Create([FromBody] CreatePlantPositionDto dto)
    {
        try
        {
            var position = await _plantPositionService.AddPlantAsync(dto);
            return CreatedAtAction(nameof(GetBySeason), new { seasonId = position.SeasonId },
                ApiResponse<PlantPositionDto>.CreatedResponse(position, "Thêm cây thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PlantPositionDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thêm cây");
            return StatusCode(500, ApiResponse<PlantPositionDto>.ErrorResponse("Lỗi khi thêm cây", 500));
        }
    }

    /// <summary>
    /// Thêm nhiều cây cùng lúc (bulk create)
    /// VD: Tạo 50 cây cà phê một lần
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult<ApiResponse<int>>> BulkCreate([FromBody] BulkCreatePlantDto dto)
    {
        try
        {
            var count = await _plantPositionService.BulkCreatePlantsAsync(dto.SeasonId, dto.Positions);
            return Ok(ApiResponse<int>.SuccessResponse(count, $"Đã tạo {count} cây"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<int>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi bulk create");
            return StatusCode(500, ApiResponse<int>.ErrorResponse("Lỗi khi tạo hàng loạt", 500));
        }
    }

    /// <summary>
    /// Cập nhật thông tin cây
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PlantPositionDto>>> Update(Guid id, [FromBody] UpdatePlantPositionDto dto)
    {
        try
        {
            var position = await _plantPositionService.UpdatePlantAsync(id, dto);
            return Ok(ApiResponse<PlantPositionDto>.SuccessResponse(position, "Cập nhật thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PlantPositionDto>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật cây {Id}", id);
            return StatusCode(500, ApiResponse<PlantPositionDto>.ErrorResponse("Lỗi khi cập nhật", 500));
        }
    }

    /// <summary>
    /// Xóa cây
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var result = await _plantPositionService.RemovePlantAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Xóa cây thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa cây {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi xóa cây", 500));
        }
    }
}

public class BulkCreatePlantDto
{
    public Guid SeasonId { get; set; }
    public List<CreatePlantPositionDto> Positions { get; set; } = new();
}
