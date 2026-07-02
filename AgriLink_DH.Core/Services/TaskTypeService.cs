using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Core.Validations;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.TaskType;

namespace AgriLink_DH.Core.Services;

public class TaskTypeService
{
    private readonly ITaskTypeRepository _taskTypeRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TaskTypeValidator _validator;

    public TaskTypeService(
        ITaskTypeRepository taskTypeRepository,
        IFarmRepository farmRepository,
        IUnitOfWork unitOfWork,
        TaskTypeValidator validator)
    {
        _taskTypeRepository = taskTypeRepository;
        _farmRepository = farmRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
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
        await _validator.ValidateCreateAsync(dto.FarmId);

        var taskType = new TaskType(dto.Name, dto.FarmId, dto.IsSystem, dto.DefaultUnit, dto.DefaultPrice);

        await _taskTypeRepository.AddAsync(taskType);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(taskType);
    }

    public async Task<TaskTypeDto> UpdateTaskTypeAsync(Guid id, UpdateTaskTypeDto dto)
    {
        var taskType = await _taskTypeRepository.GetByIdAsync(id);
        _validator.ValidateUpdate(taskType, id);

        // Model now handles IsSystem check internally, but we can keep the explicit throw if preferred
        taskType.UpdateDetails(dto.Name, dto.DefaultUnit, dto.DefaultPrice);

        _taskTypeRepository.Update(taskType);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(taskType);
    }

    public async Task<bool> DeleteTaskTypeAsync(Guid id)
    {
        var taskType = await _taskTypeRepository.GetByIdAsync(id);
        _validator.ValidateDelete(taskType, id);

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
            IsSystem = taskType.IsSystem,
            Name = taskType.Name,
            DefaultUnit = taskType.DefaultUnit,
            DefaultPrice = taskType.DefaultPrice
        };
    }
}
