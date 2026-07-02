using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Core.Validations;

public class TaskTypeValidator
{
    private readonly IFarmRepository _farmRepository;

    public TaskTypeValidator(IFarmRepository farmRepository)
    {
        _farmRepository = farmRepository;
    }

    public async Task ValidateCreateAsync(Guid? farmId)
    {
        if (farmId.HasValue)
        {
            var farm = await _farmRepository.GetByIdAsync(farmId.Value);
            if (farm == null)
            {
                throw new InvalidOperationException($"Không tìm thấy vườn với ID: {farmId}");
            }
        }
    }

    public void ValidateUpdate(TaskType? taskType, Guid id)
    {
        if (taskType == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy loại công việc với ID: {id}");
        }
    }

    public void ValidateDelete(TaskType? taskType, Guid id)
    {
        if (taskType == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy loại công việc với ID: {id}");
        }

        if (taskType.IsSystem)
        {
            throw new InvalidOperationException("Không thể xóa loại công việc chuẩn của hệ thống");
        }
    }
}
