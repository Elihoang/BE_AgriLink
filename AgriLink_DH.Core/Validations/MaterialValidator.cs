using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.Material;

namespace AgriLink_DH.Core.Validations;

public class MaterialValidator
{
    private readonly IMaterialRepository _materialRepository;

    public MaterialValidator(IMaterialRepository materialRepository)
    {
        _materialRepository = materialRepository;
    }

    public async Task ValidateCreateAsync(Guid userId, CreateMaterialDto dto)
    {
        if (await _materialRepository.ExistsByNameAndUserAsync(dto.Name, userId))
        {
            throw new ArgumentException($"Vật tư '{dto.Name}' đã tồn tại trong kho của bạn.");
        }
    }

    public async Task ValidateUpdateAsync(Guid userId, Material? material, UpdateMaterialDto dto)
    {
        if (material == null || material.OwnerUserId != userId)
        {
            throw new KeyNotFoundException("Không tìm thấy vật tư.");
        }

        // Check duplicate name if name changed
        if (!string.Equals(material.Name, dto.Name, StringComparison.OrdinalIgnoreCase))
        {
            if (await _materialRepository.ExistsByNameAndUserAsync(dto.Name, userId))
            {
                throw new ArgumentException($"Vật tư '{dto.Name}' đã tồn tại.");
            }
        }
    }

    public void ValidateDelete(Guid userId, Material? material)
    {
        if (material == null || material.OwnerUserId != userId)
        {
            throw new KeyNotFoundException("Không tìm thấy vật tư.");
        }
    }
}
