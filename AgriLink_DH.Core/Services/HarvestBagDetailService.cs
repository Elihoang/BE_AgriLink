using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.HarvestBagDetail;

namespace AgriLink_DH.Core.Services;

public class HarvestBagDetailService : BaseCachedService
{
    private readonly IHarvestBagDetailRepository _bagDetailRepository;
    private readonly IHarvestSessionRepository _harvestSessionRepository;
    private readonly ICropSeasonRepository _cropSeasonRepository;
    private readonly IUnitOfWork _unitOfWork;

    private const string CACHE_KEY_USER_PREFIX = "harvest_sessions:user:";

    public HarvestBagDetailService(
        IHarvestBagDetailRepository bagDetailRepository,
        IHarvestSessionRepository harvestSessionRepository,
        ICropSeasonRepository cropSeasonRepository,
        RedisService redisService,
        IUnitOfWork unitOfWork)
        : base(redisService)
    {
        _bagDetailRepository = bagDetailRepository;
        _harvestSessionRepository = harvestSessionRepository;
        _cropSeasonRepository = cropSeasonRepository;
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
            NetWeight = netWeight,
            IsAutoWeighed = dto.IsAutoWeighed,
            ScaleDeviceId = dto.ScaleDeviceId,
            IsDraft = false
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

    /// <summary>
    /// Lưu bao vào bản nháp (IsDraft = true).
    /// KHÔNG cộng vào TotalBags/TotalWeight của session.
    /// Bluetooth gọi API này sau mỗi lần cân ổn định.
    /// </summary>
    public async Task<HarvestBagDetailDto> AddDraftBagAsync(CreateHarvestBagDetailDto dto)
    {
        var session = await _harvestSessionRepository.GetByIdAsync(dto.SessionId);
        if (session == null)
            throw new InvalidOperationException($"Không tìm thấy phiếu thu hoạch với ID: {dto.SessionId}");

        var netWeight = dto.GrossWeight - dto.Deduction;

        var bag = new HarvestBagDetail
        {
            SessionId = dto.SessionId,
            BagIndex = dto.BagIndex,
            GrossWeight = dto.GrossWeight,
            Deduction = dto.Deduction,
            NetWeight = netWeight,
            IsAutoWeighed = dto.IsAutoWeighed,
            ScaleDeviceId = dto.ScaleDeviceId,
            IsDraft = true   // <-- chưa xác nhận
        };

        await _bagDetailRepository.AddAsync(bag);
        await _unitOfWork.SaveChangesAsync();
        // Không invalidate cache vì session chưa thay đổi

        return MapToDto(bag);
    }

    /// <summary>
    /// Xác nhận toàn bộ bao nháp của một session:
    /// - Flip IsDraft = false
    /// - Cộng dồn TotalBags / TotalWeight vào session
    /// - Invalidate cache
    /// FE gọi API này khi user nhấn "Lưu dữ liệu".
    /// </summary>
    public async Task<int> ConfirmDraftsAsync(Guid sessionId)
    {
        var session = await _harvestSessionRepository.GetWithDetailsAsync(sessionId);
        if (session == null)
            throw new InvalidOperationException($"Không tìm thấy phiếu thu hoạch với ID: {sessionId}");

        var draftBags = await _bagDetailRepository.GetDraftsBySessionIdAsync(sessionId);
        var list = draftBags.ToList();
        if (list.Count == 0) return 0;

        foreach (var bag in list)
        {
            bag.IsDraft = false;
            _bagDetailRepository.Update(bag);
            session.TotalBags += 1;
            session.TotalWeight += bag.NetWeight;
        }

        _harvestSessionRepository.Update(session);
        await _unitOfWork.SaveChangesAsync();

        await InvalidateCacheForSessionAsync(session);

        return list.Count;
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
            NetWeight = bag.NetWeight,
            IsAutoWeighed = bag.IsAutoWeighed,
            ScaleDeviceId = bag.ScaleDeviceId,
            IsDraft = bag.IsDraft
        };
    }

    /// <summary>
    /// Helper: Invalidate cache for a session
    /// </summary>
    /// <summary>
    /// Helper: Invalidate cache for a session
    /// </summary>
    private async Task InvalidateCacheForSessionAsync(HarvestSession session)
    {
        var season = session.CropSeason ?? await _cropSeasonRepository.GetByIdAsync(session.SeasonId);
        if (season?.Farm != null)
        {
            var cacheKey = $"{CACHE_KEY_USER_PREFIX}{season.Farm.OwnerUserId}";
            await InvalidateCacheAsync(cacheKey);
        }
    }
}
