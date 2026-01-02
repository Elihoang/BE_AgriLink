using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.MaterialUsage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MaterialUsagesController : ControllerBase
{
    private readonly MaterialUsageService _materialUsageService;
    private readonly ILogger<MaterialUsagesController> _logger;

    public MaterialUsagesController(
        MaterialUsageService materialUsageService,
        ILogger<MaterialUsagesController> logger)
    {
        _materialUsageService = materialUsageService;
        _logger = logger;
    }

    [HttpGet("by-season/{seasonId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MaterialUsageDto>>>> GetBySeason(Guid seasonId)
    {
        try
        {
            var usages = await _materialUsageService.GetBySeasonAsync(seasonId);
            return Ok(ApiResponse<IEnumerable<MaterialUsageDto>>.SuccessResponse(usages, "Lấy danh sách vật tư thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách vật tư của vụ mùa {SeasonId}", seasonId);
            return StatusCode(500, ApiResponse<IEnumerable<MaterialUsageDto>>.ErrorResponse("Lỗi khi lấy danh sách vật tư", 500));
        }
    }

    [HttpGet("by-farm/{farmId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MaterialUsageDto>>>> GetByFarm(Guid farmId)
    {
        try
        {
            var usages = await _materialUsageService.GetByFarmAsync(farmId);
            return Ok(ApiResponse<IEnumerable<MaterialUsageDto>>.SuccessResponse(usages, "Lấy danh sách vật tư của trang trại thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách vật tư của trang trại {FarmId}", farmId);
            return StatusCode(500, ApiResponse<IEnumerable<MaterialUsageDto>>.ErrorResponse("Lỗi khi lấy danh sách vật tư", 500));
        }
    }

    [HttpGet("total-cost/{seasonId:guid}")]
    public async Task<ActionResult<ApiResponse<decimal>>> GetTotalCost(Guid seasonId)
    {
        try
        {
            var total = await _materialUsageService.GetTotalCostAsync(seasonId);
            return Ok(ApiResponse<decimal>.SuccessResponse(total, "Tính tổng chi phí vật tư thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tính tổng chi phí vật tư");
            return StatusCode(500, ApiResponse<decimal>.ErrorResponse("Lỗi khi tính tổng chi phí vật tư", 500));
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<MaterialUsageDto>>> GetById(Guid id)
    {
        try
        {
            var usage = await _materialUsageService.GetByIdAsync(id);
            if (usage == null)
            {
                return NotFound(ApiResponse<MaterialUsageDto>.NotFoundResponse($"Không tìm thấy vật tư với ID: {id}"));
            }

            return Ok(ApiResponse<MaterialUsageDto>.SuccessResponse(usage, "Lấy thông tin vật tư thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin vật tư {Id}", id);
            return StatusCode(500, ApiResponse<MaterialUsageDto>.ErrorResponse("Lỗi khi lấy thông tin vật tư", 500));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MaterialUsageDto>>> Create([FromBody] CreateMaterialUsageDto dto)
    {
        try
        {
            var usage = await _materialUsageService.CreateUsageAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = usage.Id },
                ApiResponse<MaterialUsageDto>.CreatedResponse(usage, "Tạo vật tư mới thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<MaterialUsageDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo vật tư mới");
            return StatusCode(500, ApiResponse<MaterialUsageDto>.ErrorResponse("Lỗi khi tạo vật tư mới", 500));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<MaterialUsageDto>>> Update(Guid id, [FromBody] UpdateMaterialUsageDto dto)
    {
        try
        {
            var usage = await _materialUsageService.UpdateUsageAsync(id, dto);
            return Ok(ApiResponse<MaterialUsageDto>.SuccessResponse(usage, "Cập nhật vật tư thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<MaterialUsageDto>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật vật tư {Id}", id);
            return StatusCode(500, ApiResponse<MaterialUsageDto>.ErrorResponse("Lỗi khi cập nhật vật tư", 500));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var result = await _materialUsageService.DeleteUsageAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Xóa vật tư thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa vật tư");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi xóa vật tư", 500));
        }
    }

    [HttpPut("{id:guid}/soft-delete")]
    public async Task<ActionResult<ApiResponse<bool>>> SoftDelete(Guid id)
    {
        try
        {
            var result = await _materialUsageService.SoftDeleteUsageAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Xóa tạm vật tư thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa tạm vật tư");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi xóa tạm vật tư", 500));
        }
    }

    [HttpPut("{id:guid}/restore")]
    public async Task<ActionResult<ApiResponse<bool>>> Restore(Guid id)
    {
        try
        {
            var result = await _materialUsageService.RestoreUsageAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Khôi phục vật tư thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi khôi phục vật tư");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi khôi phục vật tư", 500));
        }
    }
}
