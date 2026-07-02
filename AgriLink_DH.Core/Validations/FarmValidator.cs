using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.Farm;

namespace AgriLink_DH.Core.Validations;

public class FarmValidator
{
    private readonly IFarmRepository _farmRepository;

    public FarmValidator(IFarmRepository farmRepository)
    {
        _farmRepository = farmRepository;
    }

    public async Task ValidateCreateAsync(CreateFarmDto dto, Guid userId)
    {
        if (await _farmRepository.ExistsByNameAndUserAsync(dto.Name, userId))
        {
            throw new InvalidOperationException($"Vườn '{dto.Name}' đã tồn tại");
        }
    }

    public void ValidateUpdate(Farm? farm, Guid id)
    {
        if (farm == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy vườn với ID: {id}");
        }
    }

    public void ValidateDelete(Farm? farm, Guid id)
    {
        if (farm == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy vườn với ID: {id}");
        }
    }
}
