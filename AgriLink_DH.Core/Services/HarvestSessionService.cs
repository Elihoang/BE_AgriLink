using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Core.Validations;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.HarvestSession;

namespace AgriLink_DH.Core.Services;

public class HarvestSessionService : BaseCachedService
{
    private readonly IHarvestSessionRepository _harvestSessionRepository;
    private readonly ICropSeasonRepository _cropSeasonRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly HarvestSessionValidator _validator;

    private const string CACHE_KEY_USER_PREFIX = "harvest_sessions:user:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30); // Cache 30 phút

    public HarvestSessionService(
        IHarvestSessionRepository harvestSessionRepository,
        ICropSeasonRepository cropSeasonRepository,
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        HarvestSessionValidator validator)
        : base(cacheService)
    {
        _harvestSessionRepository = harvestSessionRepository;
        _cropSeasonRepository = cropSeasonRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
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
        var cacheKey = $"{CACHE_KEY_USER_PREFIX}{userId}";

        return await GetOrSetCacheListAsync(
            cacheKey,
            async () =>
            {
                var sessions = await _harvestSessionRepository.GetByUserIdAsync(userId);
                return sessions.Select(MapToDto);
            },
            CacheDuration
        );
    }

    public async Task<HarvestSessionDto?> GetByIdAsync(Guid id)
    {
        var session = await _harvestSessionRepository.GetWithDetailsAsync(id);
        return session != null ? MapToDto(session) : null;
    }

    public async Task<HarvestSessionDto> CreateSessionAsync(CreateHarvestSessionDto dto)
    {
        await _validator.ValidateCreateAsync(dto.SeasonId);
        var season = await _cropSeasonRepository.GetSeasonWithDetailsAsync(dto.SeasonId);

        var session = new HarvestSession(dto.SeasonId, dto.HarvestDate.ToUniversalTime(), dto.StorageLocation);

        await _harvestSessionRepository.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        // Invalidate cache (get userId from season.Farm)
        if (season.Farm != null)
        {
            await InvalidateUserCacheAsync(season.Farm.OwnerUserId);
        }

        var resultDto = MapToDto(session);
        resultDto.SeasonName = season.Name;
        return resultDto;
    }

    public async Task<HarvestSessionDto> CreateSessionWithBagsAsync(CreateHarvestSessionWithDetailsDto dto)
    {
        await _validator.ValidateCreateAsync(dto.SeasonId);
        var season = await _cropSeasonRepository.GetSeasonWithDetailsAsync(dto.SeasonId);

        // Tạo Session
        var session = new HarvestSession(dto.SeasonId, dto.HarvestDate.ToUniversalTime(), dto.StorageLocation);

        // Thêm các bao (nếu có)
        if (dto.Bags != null && dto.Bags.Any())
        {
            var bags = new List<HarvestBagDetail>();
            
            foreach (var bagInput in dto.Bags)
            {
                var netWeight = bagInput.GrossWeight - bagInput.Deduction;
                
                var bag = new HarvestBagDetail(
                    session.Id,
                    bagInput.BagIndex,
                    bagInput.GrossWeight,
                    bagInput.Deduction
                );
                
                bags.Add(bag);
                
                // Add to collection and update total
                session.HarvestBagDetails.Add(bag);
                session.AddBag(bag.NetWeight);
            }
        }

        await _harvestSessionRepository.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        // Invalidate cache
        if (season.Farm != null)
        {
            await InvalidateUserCacheAsync(season.Farm.OwnerUserId);
        }

        var resultDto = MapToDto(session);
        resultDto.SeasonName = season.Name;
        return resultDto;
    }

    public async Task<bool> DeleteSessionAsync(Guid id)
    {
        var session = await _harvestSessionRepository.GetByIdAsync(id);
        _validator.ValidateDelete(session, id);

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
        _validator.ValidateDelete(session, id);

        session.SoftDelete();

        _harvestSessionRepository.Update(session);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RestoreSessionAsync(Guid id)
    {
        var session = await _harvestSessionRepository.GetByIdAsync(id);
        _validator.ValidateDelete(session, id);

        session.Restore();

        _harvestSessionRepository.Update(session);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Xóa cache của user khi có thay đổi harvest sessions
    /// </summary>
    private async Task InvalidateUserCacheAsync(Guid userId)
    {
        var cacheKey = $"{CACHE_KEY_USER_PREFIX}{userId}";
        await InvalidateCacheAsync(cacheKey);
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
