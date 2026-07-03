using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Core.Validations;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.MaterialUsage;

namespace AgriLink_DH.Core.Services;

public class MaterialUsageService
{
    private readonly IMaterialUsageRepository _materialUsageRepository;
    private readonly ICropSeasonRepository _cropSeasonRepository;
    private readonly IMaterialRepository _materialRepository; // Added
    private readonly IUnitOfWork _unitOfWork;
    private readonly MaterialUsageValidator _validator;

    public MaterialUsageService(
        IMaterialUsageRepository materialUsageRepository,
        ICropSeasonRepository cropSeasonRepository,
        IMaterialRepository materialRepository, // Added
        IUnitOfWork unitOfWork,
        MaterialUsageValidator validator)
    {
        _materialUsageRepository = materialUsageRepository;
        _cropSeasonRepository = cropSeasonRepository;
        _materialRepository = materialRepository; // Added
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<IEnumerable<MaterialUsageDto>> GetBySeasonAsync(Guid seasonId)
    {
        var usages = await _materialUsageRepository.GetBySeasonIdAsync(seasonId);
        return usages.Select(MapToDto);
    }

    public async Task<IEnumerable<MaterialUsageDto>> GetByFarmAsync(Guid farmId)
    {
        var usages = await _materialUsageRepository.GetByFarmIdAsync(farmId);
        
        // Debug Log to check Image loading issue
        foreach (var u in usages)
        {
             if (u.MaterialId.HasValue)
             {
                 System.Console.WriteLine($"[DEBUG-USAGE] UsageId: {u.Id}, MaterialId: {u.MaterialId}, HasMaterialObj: {u.Material != null}, ImageUrl: {u.Material?.ImageUrl}");
             }
        }

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
        await _validator.ValidateCreateUsageAsync(dto);

        var season = await _cropSeasonRepository.GetByIdAsync(dto.SeasonId);

        string materialName = dto.MaterialName ?? string.Empty;
        string unit = dto.Unit ?? string.Empty;

        // Inventory Logic
        if (dto.MaterialId.HasValue)
        {
            var material = (await _materialRepository.GetByIdAsync(dto.MaterialId.Value))!;
            
            // Deduct stock
            material.Consume(dto.Quantity);
            _materialRepository.Update(material);

            // Fill info if missing
            if (string.IsNullOrEmpty(materialName)) materialName = material.Name;
            if (string.IsNullOrEmpty(unit)) unit = material.Unit;
        }

        var totalCost = dto.Quantity * dto.UnitPrice;

        var usage = new MaterialUsage(dto.SeasonId, dto.UsageDate.ToUniversalTime(), dto.Quantity, dto.UnitPrice, dto.MaterialId, materialName, unit, dto.Note);

        await _materialUsageRepository.AddAsync(usage);
        await _unitOfWork.SaveChangesAsync();

        // usage.CropSeason = season; // Cannot assign navigation property directly
        // If material exists, we should try to reload it or set it if we have it, 
        // but for now MapToDto handles null safely. 
        // Note: Create endpoint usually returns immediately, might not include Material relation unless re-fetched.
        var dtoResult = MapToDto(usage);
        dtoResult.SeasonName = season.Name;
        return dtoResult;
    }

    public async Task<MaterialUsageDto> UpdateUsageAsync(Guid id, UpdateMaterialUsageDto dto)
    {
        var usage = await _materialUsageRepository.GetByIdAsync(id);
        await _validator.ValidateUpdateUsageAsync(usage, dto, id);

        // 1. Revert stock for old usage
        if (usage!.MaterialId.HasValue)
        {
            var oldMaterial = await _materialRepository.GetByIdAsync(usage.MaterialId.Value);
            if (oldMaterial != null)
            {
                oldMaterial.Import(usage.Quantity);
                _materialRepository.Update(oldMaterial);
            }
        }

        // 2. Prepare new data
        string materialName = dto.MaterialName ?? string.Empty;
        string unit = dto.Unit ?? string.Empty;

        // 3. Deduct stock for new usage
        if (dto.MaterialId.HasValue)
        {
            var newMaterial = (await _materialRepository.GetByIdAsync(dto.MaterialId.Value))!;
            
            newMaterial.Consume(dto.Quantity);
            _materialRepository.Update(newMaterial);

            if (string.IsNullOrEmpty(materialName)) materialName = newMaterial.Name;
            if (string.IsNullOrEmpty(unit)) unit = newMaterial.Unit;
        }

        usage.UpdateDetails(dto.UsageDate.ToUniversalTime(), dto.Quantity, dto.UnitPrice, dto.MaterialId, materialName, unit, dto.Note);

        _materialUsageRepository.Update(usage);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(usage);
    }

    public async Task<bool> DeleteUsageAsync(Guid id)
    {
        var usage = await _materialUsageRepository.GetByIdAsync(id);
        _validator.ValidateDeleteUsage(usage, id);

        // Refund stock logic
        if (usage!.MaterialId.HasValue)
        {
            var material = await _materialRepository.GetByIdAsync(usage.MaterialId.Value);
            if (material != null)
            {
                material.Import(usage.Quantity);
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
        _validator.ValidateDeleteUsage(usage, id);

        usage!.SoftDelete();

        _materialUsageRepository.Update(usage);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RestoreUsageAsync(Guid id)
    {
        var usage = await _materialUsageRepository.GetByIdAsync(id);
        _validator.ValidateDeleteUsage(usage, id);

        usage!.Restore();

        _materialUsageRepository.Update(usage);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private MaterialUsageDto MapToDto(MaterialUsage usage)
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
            MaterialImageUrl = usage.Material?.ImageUrl,
            Note = usage.Note
        };
    }
}
