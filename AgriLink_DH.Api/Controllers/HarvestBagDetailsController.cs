using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.HarvestBagDetail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HarvestBagDetailsController : ControllerBase
{
    private readonly HarvestBagDetailService _bagDetailService;
    private readonly ILogger<HarvestBagDetailsController> _logger;

    public HarvestBagDetailsController(
        HarvestBagDetailService bagDetailService,
        ILogger<HarvestBagDetailsController> logger)
    {
        _bagDetailService = bagDetailService;
        _logger = logger;
    }

    [HttpGet("by-session/{sessionId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<HarvestBagDetailDto>>>> GetBySession(Guid sessionId)
    {
        try
        {
            var bags = await _bagDetailService.GetBySessionAsync(sessionId);
            return Ok(ApiResponse<IEnumerable<HarvestBagDetailDto>>.SuccessResponse(bags, "Lấy danh sách bao thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách bao");
            return StatusCode(500, ApiResponse<IEnumerable<HarvestBagDetailDto>>.ErrorResponse("Lỗi khi lấy danh sách bao", 500));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<HarvestBagDetailDto>>> AddBag([FromBody] CreateHarvestBagDetailDto dto)
    {
        try
        {
            var bag = await _bagDetailService.AddBagAsync(dto);
            return Ok(ApiResponse<HarvestBagDetailDto>.CreatedResponse(bag, "Thêm bao thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<HarvestBagDetailDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thêm bao");
            return StatusCode(500, ApiResponse<HarvestBagDetailDto>.ErrorResponse("Lỗi khi thêm bao", 500));
        }
    }

    [HttpPost("draft")]
    public async Task<ActionResult<ApiResponse<HarvestBagDetailDto>>> AddDraftBag([FromBody] CreateHarvestBagDetailDto dto)
    {
        try
        {
            var bag = await _bagDetailService.AddDraftBagAsync(dto);
            return Ok(ApiResponse<HarvestBagDetailDto>.CreatedResponse(bag, "Thêm bao nháp thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<HarvestBagDetailDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thêm bao nháp");
            return StatusCode(500, ApiResponse<HarvestBagDetailDto>.ErrorResponse("Lỗi khi thêm bao nháp", 500));
        }
    }

    [HttpPost("confirm-drafts/{sessionId:guid}")]
    public async Task<ActionResult<ApiResponse<int>>> ConfirmDrafts(Guid sessionId)
    {
        try
        {
            var count = await _bagDetailService.ConfirmDraftsAsync(sessionId);
            return Ok(ApiResponse<int>.SuccessResponse(count, $"Xác nhận {count} bao thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<int>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi chốt bao nháp");
            return StatusCode(500, ApiResponse<int>.ErrorResponse("Lỗi khi chốt bao nháp", 500));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var result = await _bagDetailService.DeleteBagAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Xóa bao thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa bao");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi xóa bao", 500));
        }
    }
    
    [HttpPut("{id:guid}/soft-delete")]
    public async Task<ActionResult<ApiResponse<bool>>> SoftDeleteBag(Guid id)
    {
        try
        {
            var result = await _bagDetailService.SoftDeleteBagAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Xóa mềm bao thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa mềm bao {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi xóa mềm bao", 500));
        }
    }

    [HttpPut("{id:guid}/restore")]
    public async Task<ActionResult<ApiResponse<bool>>> RestoreBag(Guid id)
    {
        try
        {
            var result = await _bagDetailService.RestoreBagAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Khôi phục bao thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi khôi phục bao {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi khôi phục bao", 500));
        }
    }
}
