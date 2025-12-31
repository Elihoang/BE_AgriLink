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
    /// Lấy sơ đồ vị trí cây của 1 rẫy (CÓ CACHE - NHANH)
    /// </summary>
    [HttpGet("farm/{farmId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlantPositionDto>>>> GetByFarm(Guid farmId)
    {
        try
        {
            var positions = await _plantPositionService.GetByFarmAsync(farmId);
            return Ok(ApiResponse<IEnumerable<PlantPositionDto>>.SuccessResponse(positions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy sơ đồ cây của rẫy {FarmId}", farmId);
            return StatusCode(500, ApiResponse<IEnumerable<PlantPositionDto>>.ErrorResponse("Lỗi khi lấy sơ đồ cây", 500));
        }
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
            
            // Use GetByFarm route since SeasonId can be null
            return CreatedAtAction(
                nameof(GetByFarm), 
                new { farmId = position.FarmId },
                ApiResponse<PlantPositionDto>.CreatedResponse(position, "Thêm cây thành công"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
            return BadRequest(ApiResponse<PlantPositionDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thêm cây - FarmId: {FarmId}, ProductId: {ProductId}, Row: {Row}, Col: {Col}", 
                dto.FarmId, dto.ProductId, dto.RowNumber, dto.ColumnNumber);
            
            // Include exception details in development
            var errorMessage = $"Lỗi khi thêm cây: {ex.Message}";
            return StatusCode(500, ApiResponse<PlantPositionDto>.ErrorResponse(errorMessage, 500));
        }
    }

    /// <summary>
    /// Thêm nhiều cây cùng lúc (bulk create)
    /// VD: Tạo 50 cây cà phê một lần trong 1 rẫy
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult<ApiResponse<int>>> BulkCreate([FromBody] BulkCreatePlantDto dto)
    {
        try
        {
            var count = await _plantPositionService.BulkCreatePlantsAsync(dto.FarmId, dto.Positions);
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

    // ========================================
    // 🧪 DEBUG/TEST ENDPOINTS - CHỈ ĐỂ TEST
    // ========================================

    /// <summary>
    /// [DEBUG ONLY] Auto-generate 5 hàng x 10 cây = 50 cây cho 1 rẫy
    /// ⚠️ CHỈ DÙNG ĐỂ TEST - KHÔNG DÙNG PRODUCTION
    /// </summary>
    [HttpGet("debug/generate-test-data/{farmId:guid}")]
    public async Task<ActionResult<ApiResponse<int>>> GenerateTestData(
        Guid farmId,
        [FromQuery] Guid? productId = null,
        [FromQuery] int rows = 5,
        [FromQuery] int columnsPerRow = 10)
    {
        try
        {
            // Use default productId if not provided
            var defaultProductId = productId ?? Guid.Parse("00000000-0000-0000-0000-000000000001");

            var positions = new List<CreatePlantPositionDto>();

            // Generate: 5 rows x 10 columns = 50 plants
            for (int row = 1; row <= rows; row++)
            {
                for (int col = 1; col <= columnsPerRow; col++)
                {
                    positions.Add(new CreatePlantPositionDto
                    {
                        FarmId = farmId,
                        SeasonId = null, // Không gắn vào vụ mùa
                        RowNumber = row,
                        ColumnNumber = col,
                        ProductId = defaultProductId,
                        PlantDate = DateTime.UtcNow,
                        EstimatedYield = 45.0m + (row * col % 10), // Random-ish yield 45-54kg
                        Note = $"[AUTO-GEN] Hàng {row}, Cột {col}"
                    });
                }
            }

            var dto = new BulkCreatePlantDto
            {
                FarmId = farmId,
                Positions = positions
            };

            var count = await _plantPositionService.BulkCreatePlantsAsync(dto.FarmId, dto.Positions);
            
            return Ok(ApiResponse<int>.SuccessResponse(
                count, 
                $" [TEST] Đã tạo {count} cây ({rows} hàng x {columnsPerRow} cây)"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<int>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi generate test data cho farm {FarmId}", farmId);
            return StatusCode(500, ApiResponse<int>.ErrorResponse($"Lỗi: {ex.Message}", 500));
        }
    }
}
