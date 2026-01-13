using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.HarvestBagDetail;

namespace AgriLink_DH.Core.Services;

public class HarvestBagDetailService
{
    private readonly IHarvestBagDetailRepository _bagDetailRepository;
    private readonly IHarvestSessionRepository _harvestSessionRepository;
    private readonly ICropSeasonRepository _cropSeasonRepository;
    private readonly RedisService _redisService;
    private readonly IUnitOfWork _unitOfWork;

    private const string REDIS_KEY_PREFIX = "harvest_sessions:user:";

    public HarvestBagDetailService(
        IHarvestBagDetailRepository bagDetailRepository,
        IHarvestSessionRepository harvestSessionRepository,
        ICropSeasonRepository cropSeasonRepository,
        RedisService redisService,
        IUnitOfWork unitOfWork)
    {
        _bagDetailRepository = bagDetailRepository;
        _harvestSessionRepository = harvestSessionRepository;
        _cropSeasonRepository = cropSeasonRepository;
        _redisService = redisService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<HarvestBagDetailDto>> GetBySessionAsync(Guid sessionId)
    {
        var bags = await _bagDetailRepository.GetBySessionIdAsync(sessionId);
        return bags.Select(MapToDto);
    }

    public async Task<HarvestBagDetailDto> AddBagAsync(CreateHarvestBagDetailDto dto)
    {
        var session = await _harvestSessionRepository.GetWithDetailsAsync(dto.SessionId);
        if (session == null)
            throw new InvalidOperationException($"Không tìm thấy phiếu thu hoạch với ID: {dto.SessionId}");

        var netWeight = dto.GrossWeight - dto.Deduction;

        var bag = new HarvestBagDetail
        {
            SessionId = dto.SessionId,
            BagIndex = dto.BagIndex,
            GrossWeight = dto.GrossWeight,
            Deduction = dto.Deduction,
            NetWeight = netWeight
        };

        await _bagDetailRepository.AddAsync(bag);

        // Tự động cập nhật tổng của Session
        session.TotalBags += 1;
        session.TotalWeight += netWeight;
        _harvestSessionRepository.Update(session);

        await _unitOfWork.SaveChangesAsync();

        // Invalidate cache
        await InvalidateCacheForSessionAsync(session);

        return MapToDto(bag);
    }

    public async Task<bool> DeleteBagAsync(Guid id)
    {
        var bag = await _bagDetailRepository.GetByIdAsync(id);
        if (bag == null)
            throw new KeyNotFoundException($"Không tìm thấy bao với ID: {id}");

        var session = await _harvestSessionRepository.GetByIdAsync(bag.SessionId);
        if (session != null)
        {
            // Trừ ra khỏi tổng
            session.TotalBags -= 1;
            session.TotalWeight -= bag.NetWeight;
            _harvestSessionRepository.Update(session);
        }

        _bagDetailRepository.Remove(bag);
        await _unitOfWork.SaveChangesAsync();

        // Invalidate cache
        if (session != null)
        {
            await InvalidateCacheForSessionAsync(session);
        }

        return true;
    }
    
    public async Task<bool> SoftDeleteBagAsync(Guid id)
    {
        var bag = await _bagDetailRepository.GetByIdAsync(id);
        if (bag == null)
            throw new KeyNotFoundException($"Không tìm thấy bao với ID: {id}");

        bag.IsDeleted = true;
        bag.DeletedAt = DateTime.UtcNow;

        _bagDetailRepository.Update(bag);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
    
    public async Task<bool> RestoreBagAsync(Guid id)
    {
        var bag = await _bagDetailRepository.GetByIdAsync(id);
        if (bag == null)
            throw new KeyNotFoundException($"Không tìm thấy bao với ID: {id}");

        bag.IsDeleted = false;
        bag.DeletedAt = null;

        _bagDetailRepository.Update(bag);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static HarvestBagDetailDto MapToDto(HarvestBagDetail bag)
    {
        return new HarvestBagDetailDto
        {
            Id = bag.Id,
            SessionId = bag.SessionId,
            BagIndex = bag.BagIndex,
            GrossWeight = bag.GrossWeight,
            Deduction = bag.Deduction,
            NetWeight = bag.NetWeight
        };
    }

    /// <summary>
    /// Helper: Invalidate cache for a session
    /// </summary>
    private async Task InvalidateCacheForSessionAsync(HarvestSession session)
    {
        var season = session.CropSeason ?? await _cropSeasonRepository.GetByIdAsync(session.SeasonId);
        if (season?.Farm != null)
        {
            var cacheKey = $"{REDIS_KEY_PREFIX}{season.Farm.OwnerUserId}";
            await _redisService.DeleteAsync(cacheKey);
        }
    }
}
