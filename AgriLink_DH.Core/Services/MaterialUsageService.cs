using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.MaterialUsage;

namespace AgriLink_DH.Core.Services;

public class MaterialUsageService
{
    private readonly IMaterialUsageRepository _materialUsageRepository;
    private readonly ICropSeasonRepository _cropSeasonRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MaterialUsageService(
        IMaterialUsageRepository materialUsageRepository,
        ICropSeasonRepository cropSeasonRepository,
        IUnitOfWork unitOfWork)
    {
        _materialUsageRepository = materialUsageRepository;
        _cropSeasonRepository = cropSeasonRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<MaterialUsageDto>> GetBySeasonAsync(Guid seasonId)
    {
        var usages = await _materialUsageRepository.GetBySeasonIdAsync(seasonId);
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

        var totalCost = dto.Quantity * dto.UnitPrice;

        var usage = new MaterialUsage
        {
            SeasonId = dto.SeasonId,
            UsageDate = dto.UsageDate.ToUniversalTime(),
            MaterialName = dto.MaterialName,
            Quantity = dto.Quantity,
            Unit = dto.Unit,
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

        usage.UsageDate = dto.UsageDate.ToUniversalTime();
        usage.MaterialName = dto.MaterialName;
        usage.Quantity = dto.Quantity;
        usage.Unit = dto.Unit;
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
