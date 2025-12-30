using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.TaskType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaskTypesController : ControllerBase
{
    private readonly TaskTypeService _taskTypeService;
    private readonly ILogger<TaskTypesController> _logger;

    public TaskTypesController(TaskTypeService taskTypeService, ILogger<TaskTypesController> logger)
    {
        _taskTypeService = taskTypeService;
        _logger = logger;
    }

    [HttpGet("by-farm/{farmId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskTypeDto>>>> GetTaskTypesByFarm(Guid farmId)
    {
        try
        {
            var taskTypes = await _taskTypeService.GetByFarmIdAsync(farmId);
            return Ok(ApiResponse<IEnumerable<TaskTypeDto>>.SuccessResponse(taskTypes, "Lấy danh sách loại công việc thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách loại công việc của vườn {FarmId}", farmId);
            return StatusCode(500, ApiResponse<IEnumerable<TaskTypeDto>>.ErrorResponse("Lỗi khi lấy danh sách loại công việc", 500));
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TaskTypeDto>>> GetTaskTypeById(Guid id)
    {
        try
        {
            var taskType = await _taskTypeService.GetByIdAsync(id);
            if (taskType == null)
            {
                return NotFound(ApiResponse<TaskTypeDto>.NotFoundResponse($"Không tìm thấy loại công việc với ID: {id}"));
            }

            return Ok(ApiResponse<TaskTypeDto>.SuccessResponse(taskType, "Lấy thông tin loại công việc thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin loại công việc {Id}", id);
            return StatusCode(500, ApiResponse<TaskTypeDto>.ErrorResponse("Lỗi khi lấy thông tin loại công việc", 500));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TaskTypeDto>>> CreateTaskType([FromBody] CreateTaskTypeDto dto)
    {
        try
        {
            var taskType = await _taskTypeService.CreateTaskTypeAsync(dto);
            return CreatedAtAction(nameof(GetTaskTypeById), new { id = taskType.Id },
                ApiResponse<TaskTypeDto>.CreatedResponse(taskType, "Tạo loại công việc mới thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<TaskTypeDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo loại công việc mới");
            return StatusCode(500, ApiResponse<TaskTypeDto>.ErrorResponse("Lỗi khi tạo loại công việc mới", 500));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TaskTypeDto>>> UpdateTaskType(Guid id, [FromBody] UpdateTaskTypeDto dto)
    {
        try
        {
            var taskType = await _taskTypeService.UpdateTaskTypeAsync(id, dto);
            return Ok(ApiResponse<TaskTypeDto>.SuccessResponse(taskType, "Cập nhật loại công việc thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TaskTypeDto>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật loại công việc {Id}", id);
            return StatusCode(500, ApiResponse<TaskTypeDto>.ErrorResponse("Lỗi khi cập nhật loại công việc", 500));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTaskType(Guid id)
    {
        try
        {
            var result = await _taskTypeService.DeleteTaskTypeAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Xóa loại công việc thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa loại công việc {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi xóa loại công việc", 500));
        }
    }
}
