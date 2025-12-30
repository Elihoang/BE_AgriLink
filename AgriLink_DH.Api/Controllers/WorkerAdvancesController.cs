using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.WorkerAdvance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkerAdvancesController : ControllerBase
{
    private readonly WorkerAdvanceService _workerAdvanceService;
    private readonly ILogger<WorkerAdvancesController> _logger;

    public WorkerAdvancesController(
        WorkerAdvanceService workerAdvanceService,
        ILogger<WorkerAdvancesController> logger)
    {
        _workerAdvanceService = workerAdvanceService;
        _logger = logger;
    }

    [HttpGet("by-worker/{workerId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkerAdvanceDto>>>> GetByWorker(Guid workerId)
    {
        try
        {
            var advances = await _workerAdvanceService.GetAdvancesByWorkerAsync(workerId);
            return Ok(ApiResponse<IEnumerable<WorkerAdvanceDto>>.SuccessResponse(advances, "Lấy danh sách ứng lương thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách ứng lương của nhân công {WorkerId}", workerId);
            return StatusCode(500, ApiResponse<IEnumerable<WorkerAdvanceDto>>.ErrorResponse("Lỗi khi lấy danh sách ứng lương", 500));
        }
    }

    [HttpGet("by-season/{seasonId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkerAdvanceDto>>>> GetBySeason(Guid seasonId)
    {
        try
        {
            var advances = await _workerAdvanceService.GetAdvancesBySeasonAsync(seasonId);
            return Ok(ApiResponse<IEnumerable<WorkerAdvanceDto>>.SuccessResponse(advances, "Lấy danh sách ứng lương thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách ứng lương của vụ mùa {SeasonId}", seasonId);
            return StatusCode(500, ApiResponse<IEnumerable<WorkerAdvanceDto>>.ErrorResponse("Lỗi khi lấy danh sách ứng lương", 500));
        }
    }

    [HttpGet("by-worker-season")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkerAdvanceDto>>>> GetByWorkerAndSeason(
        [FromQuery] Guid workerId,
        [FromQuery] Guid seasonId)
    {
        try
        {
            var advances = await _workerAdvanceService.GetAdvancesByWorkerAndSeasonAsync(workerId, seasonId);
            return Ok(ApiResponse<IEnumerable<WorkerAdvanceDto>>.SuccessResponse(advances, "Lấy danh sách ứng lương thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách ứng lương");
            return StatusCode(500, ApiResponse<IEnumerable<WorkerAdvanceDto>>.ErrorResponse("Lỗi khi lấy danh sách ứng lương", 500));
        }
    }

    [HttpGet("total")]
    public async Task<ActionResult<ApiResponse<decimal>>> GetTotalAdvance(
        [FromQuery] Guid workerId,
        [FromQuery] Guid seasonId)
    {
        try
        {
            var total = await _workerAdvanceService.GetTotalAdvanceAsync(workerId, seasonId);
            return Ok(ApiResponse<decimal>.SuccessResponse(total, "Tính tổng ứng lương thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tính tổng ứng lương");
            return StatusCode(500, ApiResponse<decimal>.ErrorResponse("Lỗi khi tính tổng ứng lương", 500));
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<WorkerAdvanceDto>>> GetById(Guid id)
    {
        try
        {
            var advance = await _workerAdvanceService.GetAdvanceByIdAsync(id);
            if (advance == null)
            {
                return NotFound(ApiResponse<WorkerAdvanceDto>.NotFoundResponse($"Không tìm thấy khoản ứng với ID: {id}"));
            }

            return Ok(ApiResponse<WorkerAdvanceDto>.SuccessResponse(advance, "Lấy thông tin ứng lương thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin ứng lương {Id}", id);
            return StatusCode(500, ApiResponse<WorkerAdvanceDto>.ErrorResponse("Lỗi khi lấy thông tin ứng lương", 500));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<WorkerAdvanceDto>>> Create([FromBody] CreateWorkerAdvanceDto dto)
    {
        try
        {
            var advance = await _workerAdvanceService.CreateAdvanceAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = advance.Id },
                ApiResponse<WorkerAdvanceDto>.CreatedResponse(advance, "Tạo khoản ứng lương mới thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<WorkerAdvanceDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo khoản ứng lương mới");
            return StatusCode(500, ApiResponse<WorkerAdvanceDto>.ErrorResponse("Lỗi khi tạo khoản ứng lương mới", 500));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<WorkerAdvanceDto>>> Update(Guid id, [FromBody] UpdateWorkerAdvanceDto dto)
    {
        try
        {
            var advance = await _workerAdvanceService.UpdateAdvanceAsync(id, dto);
            return Ok(ApiResponse<WorkerAdvanceDto>.SuccessResponse(advance, "Cập nhật ứng lương thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<WorkerAdvanceDto>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật ứng lương {Id}", id);
            return StatusCode(500, ApiResponse<WorkerAdvanceDto>.ErrorResponse("Lỗi khi cập nhật ứng lương", 500));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var result = await _workerAdvanceService.DeleteAdvanceAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Xóa ứng lương thành công"));
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
            _logger.LogError(ex, "Lỗi khi xóa ứng lương {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi xóa ứng lương", 500));
        }
    }

    [HttpPut("{id:guid}/mark-deducted")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsDeducted(Guid id)
    {
        try
        {
            var result = await _workerAdvanceService.MarkAsDeductedAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Đánh dấu đã trừ lương thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đánh dấu trừ lương {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi đánh dấu trừ lương", 500));
        }
    }
}
