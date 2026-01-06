using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.MaterialUsage;

namespace AgriLink_DH.Core.Services;

public class MaterialUsageService
{
    private readonly IMaterialUsageRepository _materialUsageRepository;
    private readonly ICropSeasonRepository _cropSeasonRepository;
    private readonly IMaterialRepository _materialRepository; // Added
    private readonly IUnitOfWork _unitOfWork;

    public MaterialUsageService(
        IMaterialUsageRepository materialUsageRepository,
        ICropSeasonRepository cropSeasonRepository,
        IMaterialRepository materialRepository, // Added
        IUnitOfWork unitOfWork)
    {
        _materialUsageRepository = materialUsageRepository;
        _cropSeasonRepository = cropSeasonRepository;
        _materialRepository = materialRepository; // Added
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<MaterialUsageDto>> GetBySeasonAsync(Guid seasonId)
    {
        var usages = await _materialUsageRepository.GetBySeasonIdAsync(seasonId);
        return usages.Select(MapToDto);
    }

    public async Task<IEnumerable<MaterialUsageDto>> GetByFarmAsync(Guid farmId)
    {
        var usages = await _materialUsageRepository.GetByFarmIdAsync(farmId);
        return usages.Select(MapToDto);
    }

    public async Task<decimal> GetTotalCostAsync(Guid seasonId)
    {
        return await _materialUsageRepository.GetTotalCostBySeasonAsync(seasonId);
    }

    public async Task<MaterialUsageDto?> GetByIdAsync(Guid id)
    {
        var usage = await _materialUsageRepository.GetByIdAsync(id);
        return usage != null ? MapToDto(usage) : null;
    }

    public async Task<MaterialUsageDto> CreateUsageAsync(CreateMaterialUsageDto dto)
    {
        var season = await _cropSeasonRepository.GetByIdAsync(dto.SeasonId);
        if (season == null)
            throw new InvalidOperationException($"Không tìm thấy vụ mùa với ID: {dto.SeasonId}");

        string materialName = dto.MaterialName ?? string.Empty;
        string unit = dto.Unit ?? string.Empty;

        // Validation
        if (string.IsNullOrEmpty(materialName) && !dto.MaterialId.HasValue)
        {
             throw new ArgumentException("Vui lòng chọn vật tư từ kho hoặc nhập tên vật tư.");
        }

        // Inventory Logic
        if (dto.MaterialId.HasValue)
        {
            var material = await _materialRepository.GetByIdAsync(dto.MaterialId.Value);
            if (material == null)
                throw new KeyNotFoundException($"Không tìm thấy vật tư trong kho với ID: {dto.MaterialId}");
            
            // Deduct stock
            material.QuantityInStock -= dto.Quantity;
            _materialRepository.Update(material);

            // Fill info if missing
            if (string.IsNullOrEmpty(materialName)) materialName = material.Name;
            if (string.IsNullOrEmpty(unit)) unit = material.Unit;
        }

        var totalCost = dto.Quantity * dto.UnitPrice;

        var usage = new MaterialUsage
        {
            SeasonId = dto.SeasonId,
            UsageDate = dto.UsageDate.ToUniversalTime(),
            MaterialId = dto.MaterialId,
            MaterialName = materialName,
            Quantity = dto.Quantity,
            Unit = unit,
            UnitPrice = dto.UnitPrice,
            TotalCost = totalCost,
            Note = dto.Note
        };

        await _materialUsageRepository.AddAsync(usage);
        await _unitOfWork.SaveChangesAsync();

        usage.CropSeason = season;
        return MapToDto(usage);
    }

    public async Task<MaterialUsageDto> UpdateUsageAsync(Guid id, UpdateMaterialUsageDto dto)
    {
        var usage = await _materialUsageRepository.GetByIdAsync(id);
        if (usage == null)
            throw new KeyNotFoundException($"Không tìm thấy vật tư với ID: {id}");

        // 1. Revert stock for old usage
        if (usage.MaterialId.HasValue)
        {
            var oldMaterial = await _materialRepository.GetByIdAsync(usage.MaterialId.Value);
            if (oldMaterial != null)
            {
                oldMaterial.QuantityInStock += usage.Quantity;
                _materialRepository.Update(oldMaterial);
            }
        }

        // 2. Prepare new data
        string materialName = dto.MaterialName ?? string.Empty;
        string unit = dto.Unit ?? string.Empty;

        // Validation
        if (string.IsNullOrEmpty(materialName) && !dto.MaterialId.HasValue)
        {
             throw new ArgumentException("Vui lòng chọn vật tư từ kho hoặc nhập tên vật tư.");
        }

        // 3. Deduct stock for new usage
        if (dto.MaterialId.HasValue)
        {
            var newMaterial = await _materialRepository.GetByIdAsync(dto.MaterialId.Value);
            if (newMaterial == null)
                throw new KeyNotFoundException($"Không tìm thấy vật tư trong kho với ID: {dto.MaterialId}");
            
            newMaterial.QuantityInStock -= dto.Quantity;
            _materialRepository.Update(newMaterial);

            if (string.IsNullOrEmpty(materialName)) materialName = newMaterial.Name;
            if (string.IsNullOrEmpty(unit)) unit = newMaterial.Unit;
        }

        usage.UsageDate = dto.UsageDate.ToUniversalTime();
        usage.MaterialId = dto.MaterialId;
        usage.MaterialName = materialName;
        usage.Quantity = dto.Quantity;
        usage.Unit = unit;
        usage.UnitPrice = dto.UnitPrice;
        usage.TotalCost = dto.Quantity * dto.UnitPrice;
        usage.Note = dto.Note;

        _materialUsageRepository.Update(usage);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(usage);
    }

    public async Task<bool> DeleteUsageAsync(Guid id)
    {
        var usage = await _materialUsageRepository.GetByIdAsync(id);
        if (usage == null)
            throw new KeyNotFoundException($"Không tìm thấy vật tư với ID: {id}");

        // Refund stock logic
        if (usage.MaterialId.HasValue)
        {
            var material = await _materialRepository.GetByIdAsync(usage.MaterialId.Value);
            if (material != null)
            {
                material.QuantityInStock += usage.Quantity;
                _materialRepository.Update(material);
            }
        }

        _materialUsageRepository.Remove(usage);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SoftDeleteUsageAsync(Guid id)
    {
        var usage = await _materialUsageRepository.GetByIdAsync(id);
        if (usage == null)
            throw new KeyNotFoundException($"Không tìm thấy vật tư với ID: {id}");

        usage.IsDeleted = true;
        usage.DeletedAt = DateTime.UtcNow;

        _materialUsageRepository.Update(usage);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RestoreUsageAsync(Guid id)
    {
        var usage = await _materialUsageRepository.GetByIdAsync(id);
        if (usage == null)
            throw new KeyNotFoundException($"Không tìm thấy vật tư với ID: {id}");

        usage.IsDeleted = false;
        usage.DeletedAt = null;

        _materialUsageRepository.Update(usage);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static MaterialUsageDto MapToDto(MaterialUsage usage)
    {
        return new MaterialUsageDto
        {
            Id = usage.Id,
            SeasonId = usage.SeasonId,
            SeasonName = usage.CropSeason?.Name ?? string.Empty,
            UsageDate = usage.UsageDate,
            MaterialName = usage.MaterialName,
            Quantity = usage.Quantity,
            Unit = usage.Unit,
            UnitPrice = usage.UnitPrice,
            TotalCost = usage.TotalCost,
            Note = usage.Note
        };
    }
}
