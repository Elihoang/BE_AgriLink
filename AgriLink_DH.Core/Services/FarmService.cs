using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.Farm;

namespace AgriLink_DH.Core.Services;

public class FarmService
{
    private readonly IFarmRepository _farmRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FarmService(IFarmRepository farmRepository, IUnitOfWork unitOfWork)
    {
        _farmRepository = farmRepository;
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
        var farm = await _farmRepository.GetByIdAsync(id);
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

    private static FarmDto MapToDto(Farm farm)
    {
        return new FarmDto
        {
            Id = farm.Id,
            OwnerUserId = farm.OwnerUserId,
            Name = farm.Name,
            AreaSize = farm.AreaSize,
            AddressGps = farm.AddressGps,
            CreatedAt = farm.CreatedAt
        };
    }
}
