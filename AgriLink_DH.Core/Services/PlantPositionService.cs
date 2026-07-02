using AgriLink_DH.Core.Services;
using AgriLink_DH.Domain.Common;
using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Core.Validations;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.PlantPosition;
using System.Text.Json;

namespace AgriLink_DH.Core.Services;

public class PlantPositionService : BaseCachedService
{
    private readonly IPlantPositionRepository _plantPositionRepository;
    private readonly ICropSeasonRepository _cropSeasonRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PlantPositionValidator _validator;

    private const string CACHE_KEY_FARM_PREFIX = "plant_positions:farm:";
    private const string CACHE_KEY_SEASON_PREFIX = "plant_positions:season:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1); // Cache 1h

    public PlantPositionService(
        IPlantPositionRepository plantPositionRepository,
        ICropSeasonRepository cropSeasonRepository,
        IFarmRepository farmRepository,
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        PlantPositionValidator validator)
        : base(cacheService)
    {
        _plantPositionRepository = plantPositionRepository;
        _cropSeasonRepository = cropSeasonRepository;
        _farmRepository = farmRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    /// <summary>
    /// Lấy tất cả vị trí cây của 1 rẫy - CÓ CACHE
    /// </summary>
    public async Task<IEnumerable<PlantPositionDto>> GetByFarmAsync(Guid farmId)
    {
        var cacheKey = $"{CACHE_KEY_FARM_PREFIX}{farmId}";

        return await GetOrSetCacheListAsync(
            cacheKey,
            async () =>
            {
                var positions = await _plantPositionRepository.GetByFarmIdAsync(farmId);
                return positions.Select(MapToDto);
            },
            CacheDuration
        );
    }

    /// <summary>
    /// Lấy tất cả vị trí cây của 1 season - CÓ CACHE
    /// </summary>
    public async Task<IEnumerable<PlantPositionDto>> GetBySeasonAsync(Guid seasonId)
    {
        var cacheKey = $"{CACHE_KEY_SEASON_PREFIX}{seasonId}";

        return await GetOrSetCacheListAsync(
            cacheKey,
            async () =>
            {
                var positions = await _plantPositionRepository.GetBySeasonIdAsync(seasonId);
                return positions.Select(MapToDto);
            },
            CacheDuration
        );
    }

    /// <summary>
    /// Lấy tổng quan số lượng từng loại cây
    /// </summary>
    public async Task<Dictionary<string, int>> GetPlantSummaryAsync(Guid seasonId)
    {
        return await _plantPositionRepository.GetPlantSummaryAsync(seasonId);
    }

    /// <summary>
    /// Thêm cây mới vào vị trí
    /// </summary>
    public async Task<PlantPositionDto> AddPlantAsync(CreatePlantPositionDto dto)
    {
        await _validator.ValidateAddPlantAsync(dto);

        var position = new PlantPosition(dto.FarmId, dto.RowNumber, dto.ColumnNumber, dto.ProductId, dto.PlantDate ?? DateTime.UtcNow);
        if (dto.SeasonId.HasValue) position.AssignToSeason(dto.SeasonId.Value);
        position.UpdateDetails(dto.ProductId, dto.PlantDate ?? DateTime.UtcNow, dto.EstimatedYield, dto.Note);

        await _plantPositionRepository.AddAsync(position);
        await _unitOfWork.SaveChangesAsync();

        // Load Product for DTO
        var savedPosition = await _plantPositionRepository.GetByIdAsync(position.Id);

        // Invalidate cache if season assigned
        if (dto.SeasonId.HasValue)
        {
            await InvalidateCacheAsync($"{CACHE_KEY_SEASON_PREFIX}{dto.SeasonId.Value}");
        }

        return MapToDto(savedPosition!);
    }

    public async Task<PlantPositionDto> UpdatePlantAsync(Guid id, UpdatePlantPositionDto dto)
    {
        var position = await _plantPositionRepository.GetByIdAsync(id);
        _validator.ValidateUpdatePlant(position, id);

        position.UpdateDetails(dto.ProductId, position.PlantDate, dto.EstimatedYield, dto.Note);
        
        // Update health status if provided
        if (dto.HealthStatus.HasValue)
        {
            position.UpdateHealth(dto.HealthStatus.Value, dto.Note);
        }

        _plantPositionRepository.Update(position);
        await _unitOfWork.SaveChangesAsync();

        // Invalidate cache if season assigned
        if (position.SeasonId.HasValue)
        {
            await InvalidateCacheAsync($"{CACHE_KEY_SEASON_PREFIX}{position.SeasonId.Value}");
        }

        return MapToDto(position);
    }

    /// <summary>
    /// Xóa cây (đánh dấu là removed hoặc xóa hẳn)
    /// </summary>
    public async Task<bool> RemovePlantAsync(Guid id)
    {
        var position = await _plantPositionRepository.GetByIdAsync(id);
        _validator.ValidateRemovePlant(position, id);

        var seasonId = position.SeasonId;

        _plantPositionRepository.Remove(position);
        await _unitOfWork.SaveChangesAsync();

        // Invalidate cache if season assigned
        if (seasonId.HasValue)
        {
            await InvalidateCacheAsync($"{CACHE_KEY_SEASON_PREFIX}{seasonId.Value}");
        }

        return true;
    }

    /// <summary>
    /// Bulk create - tạo nhiều cây cùng lúc
    /// VD: Tạo 50 cây cà phê một lần trong 1 rẫy
    /// </summary>
    public async Task<int> BulkCreatePlantsAsync(Guid farmId, List<CreatePlantPositionDto> dtos)
    {
        await _validator.ValidateBulkCreatePlantsAsync(farmId);

        var positions = new List<PlantPosition>();

        foreach (var dto in dtos)
        {
            // Check duplicate position in this farm
            if (await _plantPositionRepository.PositionExistsAsync(farmId, dto.RowNumber, dto.ColumnNumber))
                continue; // Skip duplicate

            var pos = new PlantPosition(farmId, dto.RowNumber, dto.ColumnNumber, dto.ProductId, dto.PlantDate ?? DateTime.UtcNow);
            if (dto.SeasonId.HasValue) pos.AssignToSeason(dto.SeasonId.Value);
            pos.UpdateDetails(dto.ProductId, dto.PlantDate ?? DateTime.UtcNow, dto.EstimatedYield, dto.Note);
            positions.Add(pos);
        }

        foreach (var position in positions)
        {
            await _plantPositionRepository.AddAsync(position);
        }

        await _unitOfWork.SaveChangesAsync();

        // Invalidate farm cache
        await InvalidateCacheAsync($"{CACHE_KEY_FARM_PREFIX}{farmId}");

        // Invalidate season cache if any position has season
        var seasonIds = positions.Where(p => p.SeasonId.HasValue).Select(p => p.SeasonId!.Value).Distinct();
        foreach (var seasonId in seasonIds)
        {
            await InvalidateCacheAsync($"{CACHE_KEY_SEASON_PREFIX}{seasonId}");
        }

        return positions.Count;
    }

    private static PlantPositionDto MapToDto(PlantPosition position)
    {
        return new PlantPositionDto
        {
            Id = position.Id,
            FarmId = position.FarmId,
            FarmName = position.Farm?.Name ?? string.Empty,
            SeasonId = position.SeasonId,
            SeasonName = position.CropSeason?.Name,
            RowNumber = position.RowNumber,
            ColumnNumber = position.ColumnNumber,
            ProductId = position.ProductId,
            ProductName = position.Product?.Name ?? string.Empty,
            PlantDate = position.PlantDate,
            HealthStatus = position.HealthStatus, // ASP.NET Core sẽ tự serialize enum thành string trong JSON
            EstimatedYield = position.EstimatedYield,
            Note = position.Note
        };
    }
}
