using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.TaskType;

namespace AgriLink_DH.Core.Services;

public class TaskTypeService
{
    private readonly ITaskTypeRepository _taskTypeRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TaskTypeService(
        ITaskTypeRepository taskTypeRepository,
        IFarmRepository farmRepository,
        IUnitOfWork unitOfWork)
    {
        _taskTypeRepository = taskTypeRepository;
        _farmRepository = farmRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<TaskTypeDto>> GetByFarmIdAsync(Guid farmId)
    {
        var taskTypes = await _taskTypeRepository.GetByFarmIdAsync(farmId);
        return taskTypes.Select(MapToDto);
    }

    public async Task<TaskTypeDto?> GetByIdAsync(Guid id)
    {
        var taskType = await _taskTypeRepository.GetByIdAsync(id);
        return taskType != null ? MapToDto(taskType) : null;
    }

    public async Task<TaskTypeDto> CreateTaskTypeAsync(CreateTaskTypeDto dto)
    {
        var farm = await _farmRepository.GetByIdAsync(dto.FarmId);
        if (farm == null)
            throw new InvalidOperationException($"Không tìm thấy vườn với ID: {dto.FarmId}");

        var taskType = new TaskType
        {
            FarmId = dto.FarmId,
            Name = dto.Name,
            DefaultUnit = dto.DefaultUnit,
            DefaultPrice = dto.DefaultPrice
        };

        await _taskTypeRepository.AddAsync(taskType);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(taskType);
    }

    public async Task<TaskTypeDto> UpdateTaskTypeAsync(Guid id, UpdateTaskTypeDto dto)
    {
        var taskType = await _taskTypeRepository.GetByIdAsync(id);
        if (taskType == null)
            throw new KeyNotFoundException($"Không tìm thấy loại công việc với ID: {id}");

        taskType.Name = dto.Name;
        taskType.DefaultUnit = dto.DefaultUnit;
        taskType.DefaultPrice = dto.DefaultPrice;

        _taskTypeRepository.Update(taskType);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(taskType);
    }

    public async Task<bool> DeleteTaskTypeAsync(Guid id)
    {
        var taskType = await _taskTypeRepository.GetByIdAsync(id);
        if (taskType == null)
            throw new KeyNotFoundException($"Không tìm thấy loại công việc với ID: {id}");

        _taskTypeRepository.Remove(taskType);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static TaskTypeDto MapToDto(TaskType taskType)
    {
        return new TaskTypeDto
        {
            Id = taskType.Id,
            FarmId = taskType.FarmId,
            Name = taskType.Name,
            DefaultUnit = taskType.DefaultUnit,
            DefaultPrice = taskType.DefaultPrice
        };
    }
}
