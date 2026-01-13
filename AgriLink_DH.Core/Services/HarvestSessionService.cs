using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.HarvestSession;

namespace AgriLink_DH.Core.Services;

public class HarvestSessionService
{
    private readonly IHarvestSessionRepository _harvestSessionRepository;
    private readonly ICropSeasonRepository _cropSeasonRepository;
    private readonly RedisService _redisService;
    private readonly IUnitOfWork _unitOfWork;

    private const string REDIS_KEY_PREFIX = "harvest_sessions:user:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30); // Cache 30 phút

    public HarvestSessionService(
        IHarvestSessionRepository harvestSessionRepository,
        ICropSeasonRepository cropSeasonRepository,
        RedisService redisService,
        IUnitOfWork unitOfWork)
    {
        _harvestSessionRepository = harvestSessionRepository;
        _cropSeasonRepository = cropSeasonRepository;
        _redisService = redisService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<HarvestSessionDto>> GetBySeasonAsync(Guid seasonId)
    {
        var sessions = await _harvestSessionRepository.GetBySeasonIdAsync(seasonId);
        return sessions.Select(MapToDto);
    }

    /// <summary>
    /// Lấy tất cả harvest sessions của user - CÓ CACHE
    /// </summary>
    public async Task<IEnumerable<HarvestSessionDto>> GetByUserIdAsync(Guid userId)
    {
        var cacheKey = $"{REDIS_KEY_PREFIX}{userId}";

        // Try get from cache first
        var cached = await _redisService.GetAsync<List<HarvestSessionDto>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        // Cache miss - get from DB
        var sessions = await _harvestSessionRepository.GetByUserIdAsync(userId);
        var dtos = sessions.Select(MapToDto).ToList();

        // Store in cache
        await _redisService.SetAsync(cacheKey, dtos, CacheDuration);

        return dtos;
    }

    public async Task<HarvestSessionDto?> GetByIdAsync(Guid id)
    {
        var session = await _harvestSessionRepository.GetWithDetailsAsync(id);
        return session != null ? MapToDto(session) : null;
    }

    public async Task<HarvestSessionDto> CreateSessionAsync(CreateHarvestSessionDto dto)
    {
        var season = await _cropSeasonRepository.GetSeasonWithDetailsAsync(dto.SeasonId);
        if (season == null)
            throw new InvalidOperationException($"Không tìm thấy vụ mùa với ID: {dto.SeasonId}");

        var session = new HarvestSession
        {
            SeasonId = dto.SeasonId,
            HarvestDate = dto.HarvestDate.ToUniversalTime(),
            StorageLocation = dto.StorageLocation,
            TotalBags = 0,
            TotalWeight = 0
        };

        await _harvestSessionRepository.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        // Invalidate cache (get userId from season.Farm)
        if (season.Farm != null)
        {
            await InvalidateUserCacheAsync(season.Farm.OwnerUserId);
        }

        session.CropSeason = season;
        return MapToDto(session);
    }

    public async Task<HarvestSessionDto> CreateSessionWithBagsAsync(CreateHarvestSessionWithDetailsDto dto)
    {
        var season = await _cropSeasonRepository.GetSeasonWithDetailsAsync(dto.SeasonId);
        if (season == null)
            throw new InvalidOperationException($"Không tìm thấy vụ mùa với ID: {dto.SeasonId}");

        // Tạo Session
        var session = new HarvestSession
        {
            SeasonId = dto.SeasonId,
            HarvestDate = dto.HarvestDate.ToUniversalTime(),
            StorageLocation = dto.StorageLocation,
            TotalBags = 0,
            TotalWeight = 0
        };

        // Thêm các bao (nếu có)
        if (dto.Bags != null && dto.Bags.Any())
        {
            var bags = new List<HarvestBagDetail>();
            
            foreach (var bagInput in dto.Bags)
            {
                var netWeight = bagInput.GrossWeight - bagInput.Deduction;
                
                var bag = new HarvestBagDetail
                {
                    SessionId = session.Id, // ID đã được tạo từ Guid.NewGuid()
                    BagIndex = bagInput.BagIndex,
                    GrossWeight = bagInput.GrossWeight,
                    Deduction = bagInput.Deduction,
                    NetWeight = netWeight
                };
                
                bags.Add(bag);
                
                // Cập nhật tổng
                session.TotalBags += 1;
                session.TotalWeight += netWeight;
            }
            
            session.HarvestBagDetails = bags;
        }

        await _harvestSessionRepository.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        // Invalidate cache
        if (season.Farm != null)
        {
            await InvalidateUserCacheAsync(season.Farm.OwnerUserId);
        }

        session.CropSeason = season;
        return MapToDto(session);
    }

    public async Task<bool> DeleteSessionAsync(Guid id)
    {
        var session = await _harvestSessionRepository.GetByIdAsync(id);
        if (session == null)
            throw new KeyNotFoundException($"Không tìm thấy phiếu thu hoạch với ID: {id}");

        // Get userId before delete
        var season = session.CropSeason ?? await _cropSeasonRepository.GetSeasonWithDetailsAsync(session.SeasonId);
        var userId = season?.Farm?.OwnerUserId;

        _harvestSessionRepository.Remove(session);
        await _unitOfWork.SaveChangesAsync();

        // Invalidate cache
        if (userId.HasValue)
        {
            await InvalidateUserCacheAsync(userId.Value);
        }

        return true;
    }

    public async Task<bool> SoftDeleteSessionAsync(Guid id)
    {
        var session = await _harvestSessionRepository.GetByIdAsync(id);
        if (session == null)
            throw new KeyNotFoundException($"Không tìm thấy phiếu thu hoạch với ID: {id}");

        session.IsDeleted = true;
        session.DeletedAt = DateTime.UtcNow;

        _harvestSessionRepository.Update(session);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RestoreSessionAsync(Guid id)
    {
        var session = await _harvestSessionRepository.GetByIdAsync(id);
        if (session == null)
            throw new KeyNotFoundException($"Không tìm thấy phiếu thu hoạch với ID: {id}");

        session.IsDeleted = false;
        session.DeletedAt = null;

        _harvestSessionRepository.Update(session);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Xóa cache của user khi có thay đổi harvest sessions
    /// </summary>
    private async Task InvalidateUserCacheAsync(Guid userId)
    {
        var cacheKey = $"{REDIS_KEY_PREFIX}{userId}";
        await _redisService.DeleteAsync(cacheKey);
    }

    private static HarvestSessionDto MapToDto(HarvestSession session)
    {
        return new HarvestSessionDto
        {
            Id = session.Id,
            SeasonId = session.SeasonId,
            SeasonName = session.CropSeason?.Name ?? string.Empty,
            HarvestDate = session.HarvestDate,
            TotalBags = session.TotalBags,
            TotalWeight = session.TotalWeight,
            StorageLocation = session.StorageLocation
        };
    }
}
