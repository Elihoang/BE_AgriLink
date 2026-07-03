using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.MaterialUsage;

namespace AgriLink_DH.Core.Validations;

public class MaterialUsageValidator
{
    private readonly ICropSeasonRepository _cropSeasonRepository;
    private readonly IMaterialRepository _materialRepository;

    public MaterialUsageValidator(ICropSeasonRepository cropSeasonRepository, IMaterialRepository materialRepository)
    {
        _cropSeasonRepository = cropSeasonRepository;
        _materialRepository = materialRepository;
    }

    public async Task ValidateCreateUsageAsync(CreateMaterialUsageDto dto)
    {
        var season = await _cropSeasonRepository.GetByIdAsync(dto.SeasonId);
        if (season == null)
            throw new InvalidOperationException($"Không tìm thấy vụ mùa với ID: {dto.SeasonId}");

        string materialName = dto.MaterialName ?? string.Empty;

        if (string.IsNullOrEmpty(materialName) && !dto.MaterialId.HasValue)
        {
            throw new ArgumentException("Vui lòng chọn vật tư từ kho hoặc nhập tên vật tư.");
        }

        if (dto.MaterialId.HasValue)
        {
            var material = await _materialRepository.GetByIdAsync(dto.MaterialId.Value);
            if (material == null)
                throw new KeyNotFoundException($"Không tìm thấy vật tư trong kho với ID: {dto.MaterialId}");
        }
    }

    public async Task ValidateUpdateUsageAsync(MaterialUsage? usage, UpdateMaterialUsageDto dto, Guid id)
    {
        if (usage == null)
            throw new KeyNotFoundException($"Không tìm thấy vật tư với ID: {id}");

        string materialName = dto.MaterialName ?? string.Empty;

        if (string.IsNullOrEmpty(materialName) && !dto.MaterialId.HasValue)
        {
            throw new ArgumentException("Vui lòng chọn vật tư từ kho hoặc nhập tên vật tư.");
        }

        if (dto.MaterialId.HasValue)
        {
            var newMaterial = await _materialRepository.GetByIdAsync(dto.MaterialId.Value);
            if (newMaterial == null)
                throw new KeyNotFoundException($"Không tìm thấy vật tư trong kho với ID: {dto.MaterialId}");
        }
    }

    public void ValidateDeleteUsage(MaterialUsage? usage, Guid id)
    {
        if (usage == null)
            throw new KeyNotFoundException($"Không tìm thấy vật tư với ID: {id}");
    }
}
