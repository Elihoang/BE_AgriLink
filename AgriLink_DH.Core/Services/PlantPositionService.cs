using AgriLink_DH.Core.Services;
using AgriLink_DH.Domain.Common;
using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.PlantPosition;
using System.Text.Json;

namespace AgriLink_DH.Core.Services;

public class PlantPositionService
{
    private readonly IPlantPositionRepository _plantPositionRepository;
    private readonly ICropSeasonRepository _cropSeasonRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly RedisService _redisService;
    private readonly IUnitOfWork _unitOfWork;

    private const string REDIS_KEY_PREFIX = "plant_positions:season:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1); // Cache 1h

    public PlantPositionService(
        IPlantPositionRepository plantPositionRepository,
        ICropSeasonRepository cropSeasonRepository,
        IFarmRepository farmRepository,
        RedisService redisService,
        IUnitOfWork unitOfWork)
    {
        _plantPositionRepository = plantPositionRepository;
        _cropSeasonRepository = cropSeasonRepository;
        _farmRepository = farmRepository;
        _redisService = redisService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Lấy tất cả vị trí cây của 1 rẫy - CÓ CACHE
    /// </summary>
    public async Task<IEnumerable<PlantPositionDto>> GetByFarmAsync(Guid farmId)
    {
        var cacheKey = $"plant_positions:farm:{farmId}";

        // Try get from cache first
        var cached = await _redisService.GetAsync<List<PlantPositionDto>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        // Cache miss - get from DB
        var positions = await _plantPositionRepository.GetByFarmIdAsync(farmId);
        var dtos = positions.Select(MapToDto).ToList();

        // Store in cache
        await _redisService.SetAsync(cacheKey, dtos, CacheDuration);

        return dtos;
    }

    /// <summary>
    /// Lấy tất cả vị trí cây của 1 season - CÓ CACHE
    /// </summary>
    public async Task<IEnumerable<PlantPositionDto>> GetBySeasonAsync(Guid seasonId)
    {
        var cacheKey = $"{REDIS_KEY_PREFIX}{seasonId}";

        // Try get from cache first
        var cached = await _redisService.GetAsync<List<PlantPositionDto>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        // Cache miss - get from DB
        var positions = await _plantPositionRepository.GetBySeasonIdAsync(seasonId);
        var dtos = positions.Select(MapToDto).ToList();

        // Store in cache
        await _redisService.SetAsync(cacheKey, dtos, CacheDuration);

        return dtos;
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
        // Validate farm exists (REQUIRED)
        var farm = await _farmRepository.GetByIdAsync(dto.FarmId);
        if (farm == null)
            throw new InvalidOperationException($"Không tìm thấy rẫy với ID: {dto.FarmId}");

        // Validate season if provided (OPTIONAL)
        if (dto.SeasonId.HasValue)
        {
            var season = await _cropSeasonRepository.GetByIdAsync(dto.SeasonId.Value);
            if (season == null)
                throw new InvalidOperationException($"Không tìm thấy vụ mùa với ID: {dto.SeasonId}");
        }

        // Check position exists in farm
        if (await _plantPositionRepository.PositionExistsAsync(dto.FarmId, dto.RowNumber, dto.ColumnNumber))
            throw new InvalidOperationException($"Vị trí hàng {dto.RowNumber}, cột {dto.ColumnNumber} đã có cây trong rẫy này");

        var position = new PlantPosition
        {
            FarmId = dto.FarmId,
            SeasonId = dto.SeasonId,
            RowNumber = dto.RowNumber,
            ColumnNumber = dto.ColumnNumber,
            ProductId = dto.ProductId,
            PlantDate = dto.PlantDate ?? DateTime.UtcNow,
            HealthStatus = PlantHealthStatus.Healthy,
            EstimatedYield = dto.EstimatedYield,
            Note = dto.Note
        };

        await _plantPositionRepository.AddAsync(position);
        await _unitOfWork.SaveChangesAsync();

        // Load Product for DTO
        var savedPosition = await _plantPositionRepository.GetByIdAsync(position.Id);

        // Invalidate cache if season assigned
        if (dto.SeasonId.HasValue)
        {
            await InvalidateCacheAsync(dto.SeasonId.Value);
        }

        return MapToDto(savedPosition!);
    }

    public async Task<PlantPositionDto> UpdatePlantAsync(Guid id, UpdatePlantPositionDto dto)
    {
        var position = await _plantPositionRepository.GetByIdAsync(id);
        if (position == null)
            throw new KeyNotFoundException($"Không tìm thấy vị trí cây với ID: {id}");

        position.ProductId = dto.ProductId;
        
        // Update health status if provided
        if (dto.HealthStatus.HasValue)
        {
            position.HealthStatus = dto.HealthStatus.Value;
        }
        
        position.EstimatedYield = dto.EstimatedYield;
        position.Note = dto.Note;

        _plantPositionRepository.Update(position);
        await _unitOfWork.SaveChangesAsync();

        // Invalidate cache if season assigned
        if (position.SeasonId.HasValue)
        {
            await InvalidateCacheAsync(position.SeasonId.Value);
        }

        return MapToDto(position);
    }

    /// <summary>
    /// Xóa cây (đánh dấu là removed hoặc xóa hẳn)
    /// </summary>
    public async Task<bool> RemovePlantAsync(Guid id)
    {
        var position = await _plantPositionRepository.GetByIdAsync(id);
        if (position == null)
            throw new KeyNotFoundException($"Không tìm thấy vị trí cây với ID: {id}");

        var seasonId = position.SeasonId;

        _plantPositionRepository.Remove(position);
        await _unitOfWork.SaveChangesAsync();

        // Invalidate cache if season assigned
        if (seasonId.HasValue)
        {
            await InvalidateCacheAsync(seasonId.Value);
        }

        return true;
    }

    /// <summary>
    /// Bulk create - tạo nhiều cây cùng lúc
    /// VD: Tạo 50 cây cà phê một lần trong 1 rẫy
    /// </summary>
    public async Task<int> BulkCreatePlantsAsync(Guid farmId, List<CreatePlantPositionDto> dtos)
    {
        // Validate farm
        var farm = await _farmRepository.GetByIdAsync(farmId);
        if (farm == null)
            throw new InvalidOperationException($"Không tìm thấy rẫy với ID: {farmId}");

        var positions = new List<PlantPosition>();

        foreach (var dto in dtos)
        {
            // Check duplicate position in this farm
            if (await _plantPositionRepository.PositionExistsAsync(farmId, dto.RowNumber, dto.ColumnNumber))
                continue; // Skip duplicate

            positions.Add(new PlantPosition
            {
                FarmId = farmId,
                SeasonId = dto.SeasonId, // Optional
                RowNumber = dto.RowNumber,
                ColumnNumber = dto.ColumnNumber,
                ProductId = dto.ProductId,
                PlantDate = dto.PlantDate ?? DateTime.UtcNow,
                HealthStatus = PlantHealthStatus.Healthy,
                EstimatedYield = dto.EstimatedYield,
                Note = dto.Note
            });
        }

        foreach (var position in positions)
        {
            await _plantPositionRepository.AddAsync(position);
        }

        await _unitOfWork.SaveChangesAsync();

        // Invalidate farm cache
        await _redisService.DeleteAsync($"plant_positions:farm:{farmId}");

        // Invalidate season cache if any position has season
        var seasonIds = positions.Where(p => p.SeasonId.HasValue).Select(p => p.SeasonId!.Value).Distinct();
        foreach (var seasonId in seasonIds)
        {
            await InvalidateCacheAsync(seasonId);
        }

        return positions.Count;
    }

    /// <summary>
    /// Xóa cache khi có thay đổi
    /// </summary>
    private async Task InvalidateCacheAsync(Guid seasonId)
    {
        var cacheKey = $"{REDIS_KEY_PREFIX}{seasonId}";
        await _redisService.DeleteAsync(cacheKey);
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
