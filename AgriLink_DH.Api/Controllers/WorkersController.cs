using AgriLink_DH.Api.Extensions;
using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.Worker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkersController : ControllerBase
{
    private readonly WorkerService _workerService;
    private readonly ILogger<WorkersController> _logger;

    public WorkersController(WorkerService workerService, ILogger<WorkersController> logger)
    {
        _workerService = workerService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkerDto>>>> GetWorkers()
    {
        try
        {
            var workers = await _workerService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<WorkerDto>>.SuccessResponse(workers, "Lấy danh sách nhân công thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách nhân công");
            return StatusCode(500, ApiResponse<IEnumerable<WorkerDto>>.ErrorResponse("Lỗi khi lấy danh sách nhân công", 500));
        }
    }

    [HttpGet("by-season/{seasonId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkerDto>>>> GetWorkersBySeason(Guid seasonId)
    {
        try
        {
            var workers = await _workerService.GetWorkersBySeasonIdAsync(seasonId);
            return Ok(ApiResponse<IEnumerable<WorkerDto>>.SuccessResponse(workers, "Lấy danh sách nhân công theo vụ mùa thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách nhân công theo vụ mùa {SeasonId}", seasonId);
            return StatusCode(500, ApiResponse<IEnumerable<WorkerDto>>.ErrorResponse("Lỗi khi lấy danh sách nhân công theo vụ mùa", 500));
        }
    }

    [HttpGet("by-farm/{farmId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkerDto>>>> GetWorkersByFarm(Guid farmId)
    {
        try
        {
            var workers = await _workerService.GetWorkersByFarmIdAsync(farmId);
            return Ok(ApiResponse<IEnumerable<WorkerDto>>.SuccessResponse(workers, "Lấy danh sách nhân công theo trang trại thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách nhân công theo trang trại {FarmId}", farmId);
            return StatusCode(500, ApiResponse<IEnumerable<WorkerDto>>.ErrorResponse("Lỗi khi lấy danh sách nhân công theo trang trại", 500));
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<WorkerDto>>> GetWorkerById(Guid id)
    {
        try
        {
            var worker = await _workerService.GetByIdAsync(id);
            if (worker == null)
            {
                return NotFound(ApiResponse<WorkerDto>.NotFoundResponse($"Không tìm thấy nhân công với ID: {id}"));
            }

            return Ok(ApiResponse<WorkerDto>.SuccessResponse(worker, "Lấy thông tin nhân công thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin nhân công {Id}", id);
            return StatusCode(500, ApiResponse<WorkerDto>.ErrorResponse("Lỗi khi lấy thông tin nhân công", 500));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<WorkerDto>>> CreateWorker([FromBody] CreateWorkerDto dto)
    {
        try
        {
            var worker = await _workerService.CreateWorkerAsync(dto);
            return CreatedAtAction(nameof(GetWorkerById), new { id = worker.Id },
                ApiResponse<WorkerDto>.CreatedResponse(worker, "Tạo nhân công mới thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<WorkerDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo nhân công mới");
            return StatusCode(500, ApiResponse<WorkerDto>.ErrorResponse("Lỗi khi tạo nhân công mới", 500));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<WorkerDto>>> UpdateWorker(Guid id, [FromBody] UpdateWorkerDto dto)
    {
        try
        {
            var worker = await _workerService.UpdateWorkerAsync(id, dto);
            return Ok(ApiResponse<WorkerDto>.SuccessResponse(worker, "Cập nhật nhân công thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<WorkerDto>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật nhân công {Id}", id);
            return StatusCode(500, ApiResponse<WorkerDto>.ErrorResponse("Lỗi khi cập nhật nhân công", 500));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteWorker(Guid id)
    {
        try
        {
            var result = await _workerService.DeleteWorkerAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Xóa nhân công thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa nhân công {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi xóa nhân công", 500));
        }
    }
}
