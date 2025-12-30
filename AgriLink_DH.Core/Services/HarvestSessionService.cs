using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.HarvestSession;

namespace AgriLink_DH.Core.Services;

public class HarvestSessionService
{
    private readonly IHarvestSessionRepository _harvestSessionRepository;
    private readonly ICropSeasonRepository _cropSeasonRepository;
    private readonly IUnitOfWork _unitOfWork;

    public HarvestSessionService(
        IHarvestSessionRepository harvestSessionRepository,
        ICropSeasonRepository cropSeasonRepository,
        IUnitOfWork unitOfWork)
    {
        _harvestSessionRepository = harvestSessionRepository;
        _cropSeasonRepository = cropSeasonRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<HarvestSessionDto>> GetBySeasonAsync(Guid seasonId)
    {
        var sessions = await _harvestSessionRepository.GetBySeasonIdAsync(seasonId);
        return sessions.Select(MapToDto);
    }

    public async Task<HarvestSessionDto?> GetByIdAsync(Guid id)
    {
        var session = await _harvestSessionRepository.GetWithDetailsAsync(id);
        return session != null ? MapToDto(session) : null;
    }

    public async Task<HarvestSessionDto> CreateSessionAsync(CreateHarvestSessionDto dto)
    {
        var season = await _cropSeasonRepository.GetByIdAsync(dto.SeasonId);
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

        session.CropSeason = season;
        return MapToDto(session);
    }

    public async Task<HarvestSessionDto> CreateSessionWithBagsAsync(CreateHarvestSessionWithDetailsDto dto)
    {
        var season = await _cropSeasonRepository.GetByIdAsync(dto.SeasonId);
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

        session.CropSeason = season;
        return MapToDto(session);
    }

    public async Task<bool> DeleteSessionAsync(Guid id)
    {
        var session = await _harvestSessionRepository.GetByIdAsync(id);
        if (session == null)
            throw new KeyNotFoundException($"Không tìm thấy phiếu thu hoạch với ID: {id}");

        _harvestSessionRepository.Remove(session);
        await _unitOfWork.SaveChangesAsync();

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
