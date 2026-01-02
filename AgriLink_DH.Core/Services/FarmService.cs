using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.Farm;
using AgriLink_DH.Share.DTOs.CropSeason;
using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Core.Services;

public class FarmService
{
    private readonly IFarmRepository _farmRepository;
    private readonly ICropSeasonRepository _cropSeasonRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FarmService(
        IFarmRepository farmRepository, 
        ICropSeasonRepository cropSeasonRepository,
        IUnitOfWork unitOfWork)
    {
        _farmRepository = farmRepository;
        _cropSeasonRepository = cropSeasonRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<FarmDto>> GetAllFarmsAsync()
    {
        var farms = await _farmRepository.GetAllAsync();
        return farms.Select(MapToDto);
    }

    public async Task<IEnumerable<FarmDto>> GetFarmsByUserIdAsync(Guid userId)
    {
        var farms = await _farmRepository.GetFarmsByUserIdAsync(userId);
        return farms.Select(MapToDto);
    }

    public async Task<FarmDto?> GetFarmByIdAsync(Guid id)
    {
        var farm = await _farmRepository.GetFarmWithDetailsByIdAsync(id);
        return farm != null ? MapToDto(farm) : null;
    }

    public async Task<FarmDto> CreateFarmAsync(CreateFarmDto dto, Guid userId)
    {
        if (await _farmRepository.ExistsByNameAndUserAsync(dto.Name, userId))
        {
            throw new InvalidOperationException($"Vườn '{dto.Name}' đã tồn tại");
        }

        var farm = new Farm
        {
            OwnerUserId = userId,
            Name = dto.Name,
            AreaSize = dto.AreaSize,
            AddressGps = dto.AddressGps,
            GoogleMapsUrl = dto.GoogleMapsUrl,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            ImageUrl = dto.ImageUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _farmRepository.AddAsync(farm);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(farm);
    }

    public async Task<FarmDto> UpdateFarmAsync(Guid id, UpdateFarmDto dto)
    {
        var farm = await _farmRepository.GetByIdAsync(id);
        if (farm == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy vườn với ID: {id}");
        }

        farm.Name = dto.Name;
        farm.AreaSize = dto.AreaSize;
        farm.AddressGps = dto.AddressGps;
        farm.GoogleMapsUrl = dto.GoogleMapsUrl;
        farm.Latitude = dto.Latitude;
        farm.Longitude = dto.Longitude;
        farm.ImageUrl = dto.ImageUrl;

        _farmRepository.Update(farm);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(farm);
    }

    public async Task<bool> DeleteFarmAsync(Guid id)
    {
        var farm = await _farmRepository.GetByIdAsync(id);
        if (farm == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy vườn với ID: {id}");
        }

        _farmRepository.Remove(farm);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SoftDeleteFarmAsync(Guid id)
    {
        var farm = await _farmRepository.GetByIdAsync(id);
        if (farm == null)
            throw new KeyNotFoundException($"Không tìm thấy vườn với ID: {id}");

        farm.IsDeleted = true;
        farm.DeletedAt = DateTime.UtcNow;

        _farmRepository.Update(farm);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RestoreFarmAsync(Guid id)
    {
        var farm = await _farmRepository.GetByIdAsync(id);
        if (farm == null)
            throw new KeyNotFoundException($"Không tìm thấy vườn với ID: {id}");

        farm.IsDeleted = false;
        farm.DeletedAt = null;

        _farmRepository.Update(farm);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Get farms with active crop seasons for a user
    /// </summary>
    public async Task<IEnumerable<FarmWithSeasonDto>> GetFarmsWithActiveSeasonsAsync(Guid userId)
    {
        var farms = await _farmRepository.GetFarmsByUserIdAsync(userId);
        var result = new List<FarmWithSeasonDto>();

        foreach (var farm in farms)
        {
            var activeSeasons = await _cropSeasonRepository.GetActiveSeasonsAsync();
            var farmSeasons = activeSeasons.Where(s => s.FarmId == farm.Id).ToList();

            result.Add(MapToFarmWithSeasonDto(farm, farmSeasons));
        }

        return result;
    }

    private static FarmDto MapToDto(Farm farm)
    {
        return new FarmDto
        {
            Id = farm.Id,
            OwnerUserId = farm.OwnerUserId,
            OwnerName = farm.Owner?.FullName ?? farm.Owner?.Username ?? "Unknown Owner",
            Name = farm.Name,
            AreaSize = farm.AreaSize,
            AddressGps = farm.AddressGps,
            GoogleMapsUrl = farm.GoogleMapsUrl,
            Latitude = farm.Latitude,
            Longitude = farm.Longitude,
            ImageUrl = farm.ImageUrl,
            CreatedAt = farm.CreatedAt
        };
    }

    private static FarmWithSeasonDto MapToFarmWithSeasonDto(Farm farm, List<CropSeason> seasons)
    {
        return new FarmWithSeasonDto
        {
            Id = farm.Id,
            OwnerUserId = farm.OwnerUserId,
            Name = farm.Name,
            AreaSize = farm.AreaSize,
            AddressGps = farm.AddressGps,
            GoogleMapsUrl = farm.GoogleMapsUrl,
            Latitude = farm.Latitude,
            Longitude = farm.Longitude,
            CreatedAt = farm.CreatedAt,
            CropSeasons = seasons.Select(s => new CropSeasonDto
            {
                Id = s.Id,
                FarmId = s.FarmId,
                FarmName = farm.Name,
                ProductId = s.ProductId,
                ProductName = s.Product?.Name ?? "",
                Name = s.Name,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                Status = s.Status,
                StatusText = s.Status.ToString(),
                Note = s.Note
            }).ToList()
        };
    }
}
