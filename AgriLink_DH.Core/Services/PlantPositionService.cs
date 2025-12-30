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
    private readonly RedisService _redisService;
    private readonly IUnitOfWork _unitOfWork;

    private const string REDIS_KEY_PREFIX = "plant_positions:season:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1); // Cache 1h

    public PlantPositionService(
        IPlantPositionRepository plantPositionRepository,
        ICropSeasonRepository cropSeasonRepository,
        RedisService redisService,
        IUnitOfWork unitOfWork)
    {
        _plantPositionRepository = plantPositionRepository;
        _cropSeasonRepository = cropSeasonRepository;
        _redisService = redisService;
        _unitOfWork = unitOfWork;
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
        // Validate season exists
        var season = await _cropSeasonRepository.GetByIdAsync(dto.SeasonId);
        if (season == null)
            throw new InvalidOperationException($"Không tìm thấy vụ mùa với ID: {dto.SeasonId}");

        // Check position exists
        if (await _plantPositionRepository.PositionExistsAsync(dto.SeasonId, dto.RowNumber, dto.ColumnNumber))
            throw new InvalidOperationException($"Vị trí hàng {dto.RowNumber}, cột {dto.ColumnNumber} đã có cây");

        var position = new PlantPosition
        {
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

        // Invalidate cache
        await InvalidateCacheAsync(dto.SeasonId);

        return MapToDto(savedPosition!);
    }

    public async Task<PlantPositionDto> UpdatePlantAsync(Guid id, UpdatePlantPositionDto dto)
    {
        var position = await _plantPositionRepository.GetByIdAsync(id);
        if (position == null)
            throw new KeyNotFoundException($"Không tìm thấy vị trí cây với ID: {id}");

        position.ProductId = dto.ProductId;
        
        // Parse HealthStatus string to enum
        if (!string.IsNullOrEmpty(dto.HealthStatus) && Enum.TryParse<PlantHealthStatus>(dto.HealthStatus, true, out var healthStatus))
        {
            position.HealthStatus = healthStatus;
        }
        
        position.EstimatedYield = dto.EstimatedYield;
        position.Note = dto.Note;

        _plantPositionRepository.Update(position);
        await _unitOfWork.SaveChangesAsync();

        // Invalidate cache
        await InvalidateCacheAsync(position.SeasonId);

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

        // Invalidate cache
        await InvalidateCacheAsync(seasonId);

        return true;
    }

    /// <summary>
    /// Bulk create - tạo nhiều cây cùng lúc
    /// VD: Tạo 50 cây cà phê một lần
    /// </summary>
    public async Task<int> BulkCreatePlantsAsync(Guid seasonId, List<CreatePlantPositionDto> dtos)
    {
        // Validate season
        var season = await _cropSeasonRepository.GetByIdAsync(seasonId);
        if (season == null)
            throw new InvalidOperationException($"Không tìm thấy vụ mùa với ID: {seasonId}");

        var positions = new List<PlantPosition>();

        foreach (var dto in dtos)
        {
            // Check duplicate position
            if (await _plantPositionRepository.PositionExistsAsync(seasonId, dto.RowNumber, dto.ColumnNumber))
                continue; // Skip duplicate

            positions.Add(new PlantPosition
            {
                SeasonId = seasonId,
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

        // Invalidate cache
        await InvalidateCacheAsync(seasonId);

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
            SeasonId = position.SeasonId,
            RowNumber = position.RowNumber,
            ColumnNumber = position.ColumnNumber,
            ProductId = position.ProductId,
            ProductName = position.Product?.Name ?? string.Empty,
            PlantDate = position.PlantDate,
            HealthStatus = position.HealthStatus.ToString(),
            EstimatedYield = position.EstimatedYield,
            Note = position.Note
        };
    }
}
