using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.DailyWorkLog;
using AgriLink_DH.Share.DTOs.WorkAssignment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DailyWorkLogsController : ControllerBase
{
    private readonly DailyWorkLogService _dailyWorkLogService;
    private readonly ILogger<DailyWorkLogsController> _logger;

    public DailyWorkLogsController(DailyWorkLogService dailyWorkLogService, ILogger<DailyWorkLogsController> logger)
    {
        _dailyWorkLogService = dailyWorkLogService;
        _logger = logger;
    }

    [HttpGet("by-season/{seasonId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DailyWorkLogDto>>>> GetLogsBySeason(Guid seasonId)
    {
        try
        {
            var logs = await _dailyWorkLogService.GetLogsBySeasonAsync(seasonId);
            return Ok(ApiResponse<IEnumerable<DailyWorkLogDto>>.SuccessResponse(logs, "Lấy danh sách nhật ký thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách nhật ký của vụ mùa {SeasonId}", seasonId);
            return StatusCode(500, ApiResponse<IEnumerable<DailyWorkLogDto>>.ErrorResponse("Lỗi khi lấy danh sách nhật ký", 500));
        }
    }

    [HttpGet("task-type/{taskTypeId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DailyWorkLogDto>>>> GetLogsByTaskType(Guid taskTypeId)
    {
        try
        {
            var logs = await _dailyWorkLogService.GetLogsByTaskTypeAsync(taskTypeId);
            return Ok(ApiResponse<IEnumerable<DailyWorkLogDto>>.SuccessResponse(logs, "Lấy danh sách nhật ký theo loại công việc thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách nhật ký theo TaskType {TaskTypeId}", taskTypeId);
            return StatusCode(500, ApiResponse<IEnumerable<DailyWorkLogDto>>.ErrorResponse("Lỗi khi lấy danh sách nhật ký", 500));
        }
    }

    [HttpGet("farm/{farmId:guid}/task-type/{taskTypeId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DailyWorkLogDto>>>> GetLogsByFarmAndTaskType(Guid farmId, Guid taskTypeId)
    {
        try
        {
            var logs = await _dailyWorkLogService.GetLogsByFarmAndTaskTypeAsync(farmId, taskTypeId);
            return Ok(ApiResponse<IEnumerable<DailyWorkLogDto>>.SuccessResponse(logs, "Lấy danh sách nhật ký theo trang trại và loại công việc thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách nhật ký theo Farm {FarmId} và Task {TaskTypeId}", farmId, taskTypeId);
            return StatusCode(500, ApiResponse<IEnumerable<DailyWorkLogDto>>.ErrorResponse("Lỗi server", 500));
        }
    }

    [HttpGet("season/{seasonId:guid}/task-type/{taskTypeId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DailyWorkLogDto>>>> GetLogsBySeasonAndTaskType(Guid seasonId, Guid taskTypeId)
    {
        try
        {
            var logs = await _dailyWorkLogService.GetLogsBySeasonAndTaskTypeAsync(seasonId, taskTypeId);
            return Ok(ApiResponse<IEnumerable<DailyWorkLogDto>>.SuccessResponse(logs, "Lấy danh sách nhật ký theo vụ mùa và loại công việc thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách nhật ký theo Season {SeasonId} và Task {TaskTypeId}", seasonId, taskTypeId);
            return StatusCode(500, ApiResponse<IEnumerable<DailyWorkLogDto>>.ErrorResponse("Lỗi server", 500));
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DailyWorkLogDto>>> GetLogById(Guid id)
    {
        try
        {
            var log = await _dailyWorkLogService.GetLogByIdAsync(id);
            if (log == null)
            {
                return NotFound(ApiResponse<DailyWorkLogDto>.NotFoundResponse($"Không tìm thấy nhật ký với ID: {id}"));
            }

            return Ok(ApiResponse<DailyWorkLogDto>.SuccessResponse(log, "Lấy thông tin nhật ký thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin nhật ký {Id}", id);
            return StatusCode(500, ApiResponse<DailyWorkLogDto>.ErrorResponse("Lỗi khi lấy thông tin nhật ký", 500));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<DailyWorkLogDto>>> CreateLog([FromBody] CreateDailyWorkLogDto dto)
    {
        try
        {
            var log = await _dailyWorkLogService.CreateLogAsync(dto);
            return CreatedAtAction(nameof(GetLogById), new { id = log.Id },
                ApiResponse<DailyWorkLogDto>.CreatedResponse(log, "Tạo nhật ký mới thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<DailyWorkLogDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo nhật ký mới");
            return StatusCode(500, ApiResponse<DailyWorkLogDto>.ErrorResponse("Lỗi khi tạo nhật ký mới", 500));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DailyWorkLogDto>>> UpdateLog(Guid id, [FromBody] UpdateDailyWorkLogDto dto)
    {
        try
        {
            var log = await _dailyWorkLogService.UpdateLogAsync(id, dto);
            return Ok(ApiResponse<DailyWorkLogDto>.SuccessResponse(log, "Cập nhật nhật ký thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DailyWorkLogDto>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật nhật ký {Id}", id);
            return StatusCode(500, ApiResponse<DailyWorkLogDto>.ErrorResponse("Lỗi khi cập nhật nhật ký", 500));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteLog(Guid id)
    {
        try
        {
            var result = await _dailyWorkLogService.DeleteLogAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Xóa nhật ký thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa nhật ký {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi xóa nhật ký", 500));
        }
    }

    // --- ASSIGNMENTS ---

    [HttpGet("{logId:guid}/assignments")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WorkAssignmentDto>>>> GetAssignments(Guid logId)
    {
        try
        {
            var assignments = await _dailyWorkLogService.GetAssignmentsByLogAsync(logId);
            return Ok(ApiResponse<IEnumerable<WorkAssignmentDto>>.SuccessResponse(assignments, "Lấy danh sách chấm công thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách chấm công của nhật ký {LogId}", logId);
            return StatusCode(500, ApiResponse<IEnumerable<WorkAssignmentDto>>.ErrorResponse("Lỗi khi lấy danh sách chấm công", 500));
        }
    }

    [HttpPost("assignments")]
    public async Task<ActionResult<ApiResponse<WorkAssignmentDto>>> AddAssignment([FromBody] CreateWorkAssignmentDto dto)
    {
        try
        {
            var assignment = await _dailyWorkLogService.AddAssignmentAsync(dto);
            return Ok(ApiResponse<WorkAssignmentDto>.CreatedResponse(assignment, "Thêm chấm công thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<WorkAssignmentDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thêm chấm công");
            return StatusCode(500, ApiResponse<WorkAssignmentDto>.ErrorResponse("Lỗi khi thêm chấm công", 500));
        }
    }

    [HttpDelete("assignments/{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveAssignment(Guid id)
    {
        try
        {
            var result = await _dailyWorkLogService.RemoveAssignmentAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Xóa chấm công thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa chấm công {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi xóa chấm công", 500));
        }
    }
}
