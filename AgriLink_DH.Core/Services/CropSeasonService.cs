using AgriLink_DH.Domain.Common;
using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.CropSeason;
using AgriLink_DH.Share.Extensions;

namespace AgriLink_DH.Core.Services;

public class CropSeasonService
{
    private readonly ICropSeasonRepository _cropSeasonRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CropSeasonService(
        ICropSeasonRepository cropSeasonRepository,
        IFarmRepository farmRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _cropSeasonRepository = cropSeasonRepository;
        _farmRepository = farmRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CropSeasonDto>> GetAllSeasonsAsync()
    {
        var seasons = await _cropSeasonRepository.GetAllAsync();
        return seasons.Select(MapToDto);
    }

    public async Task<IEnumerable<CropSeasonDto>> GetSeasonsByFarmIdAsync(Guid farmId)
    {
        var seasons = await _cropSeasonRepository.GetSeasonsByFarmIdAsync(farmId);
        return seasons.Select(MapToDto);
    }

    public async Task<IEnumerable<CropSeasonDto>> GetActiveSeasonsAsync()
    {
        var seasons = await _cropSeasonRepository.GetActiveSeasonsAsync();
        return seasons.Select(MapToDto);
    }

    /// <summary>
    /// Lấy tất cả seasons của các farms thuộc user  
    /// OPTIMIZED: Chỉ 1 query thay vì N+1
    /// </summary>
    public async Task<IEnumerable<CropSeasonDto>> GetSeasonsByUserIdAsync(Guid userId)
    {
        // Single query với JOIN - không còn N+1!
        var userSeasons = await _cropSeasonRepository.GetSeasonsByUserIdAsync(userId);
        return userSeasons.Select(MapToDto).OrderByDescending(s => s.StartDate);
    }

    public async Task<CropSeasonDto?> GetSeasonByIdAsync(Guid id)
    {
        var season = await _cropSeasonRepository.GetSeasonWithDetailsAsync(id);
        return season != null ? MapToDto(season) : null;
    }

    public async Task<CropSeasonDto> CreateSeasonAsync(CreateCropSeasonDto dto)
    {
        var farm = await _farmRepository.GetByIdAsync(dto.FarmId);
        if (farm == null)
        {
            throw new InvalidOperationException($"Không tìm thấy vườn với ID: {dto.FarmId}");
        }

        var product = await _productRepository.GetByIdAsync(dto.ProductId);
        if (product == null)
        {
            throw new InvalidOperationException($"Không tìm thấy sản phẩm với ID: {dto.ProductId}");
        }

        var season = new CropSeason
        {
            FarmId = dto.FarmId,
            ProductId = dto.ProductId,
            Name = dto.Name,
            StartDate = dto.StartDate.HasValue
                ? DateTime.SpecifyKind(dto.StartDate.Value, DateTimeKind.Utc)
                : null,
            EndDate = dto.EndDate.HasValue
                ? DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Utc)
                : null,
            Status = SeasonStatus.Active,
            Note = dto.Note
        };

        await _cropSeasonRepository.AddAsync(season);
        await _unitOfWork.SaveChangesAsync();

        season.Farm = farm;
        season.Product = product;

        return MapToDto(season);
    }

    public async Task<CropSeasonDto> UpdateSeasonAsync(Guid id, UpdateCropSeasonDto dto)
    {
        var season = await _cropSeasonRepository.GetSeasonWithDetailsAsync(id);
        if (season == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy vụ mùa với ID: {id}");
        }

        season.Name = dto.Name;
        season.StartDate = dto.StartDate.HasValue 
            ? DateTime.SpecifyKind(dto.StartDate.Value, DateTimeKind.Utc) 
            : null;
        season.EndDate = dto.EndDate.HasValue
            ? DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Utc)
            : null;
        
        if (dto.Status.HasValue)
        {
            season.Status = dto.Status.Value;
        }
        
        season.Note = dto.Note;

        _cropSeasonRepository.Update(season);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(season);
    }

    public async Task<bool> DeleteSeasonAsync(Guid id)
    {
        var season = await _cropSeasonRepository.GetByIdAsync(id);
        if (season == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy vụ mùa với ID: {id}");
        }

        _cropSeasonRepository.Remove(season);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Cập nhật giai đoạn sinh trưởng của vụ mùa
    /// </summary>
    public async Task<CropSeasonDto> UpdateStageAsync(Guid id, UpdateStageDto dto)
    {
        var season = await _cropSeasonRepository.GetSeasonWithDetailsAsync(id);
        if (season == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy vụ mùa với ID: {id}");
        }

        season.CurrentStage = dto.Stage;
        season.StageChangedAt = DateTime.UtcNow;
        season.StageNotes = dto.StageNotes;

        _cropSeasonRepository.Update(season);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(season);
    }

    private static CropSeasonDto MapToDto(CropSeason season)
    {
        return new CropSeasonDto
        {
            Id = season.Id,
            FarmId = season.FarmId,
            FarmName = season.Farm?.Name ?? string.Empty,
            ProductId = season.ProductId,
            ProductName = season.Product?.Name ?? string.Empty,
            ProductImageUrl = season.Product?.ImageUrl,  // Map product image
            Name = season.Name,
            StartDate = season.StartDate,
            EndDate = season.EndDate,
            Status = season.Status,
            StatusText = season.Status.ToVietnamese(),
            Note = season.Note,
            // Stage tracking
            CurrentStage = season.CurrentStage,
            StageChangedAt = season.StageChangedAt,
            StageNotes = season.StageNotes
        };
    }
}
