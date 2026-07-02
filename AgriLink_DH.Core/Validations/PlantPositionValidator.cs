using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.PlantPosition;

namespace AgriLink_DH.Core.Validations;

public class PlantPositionValidator
{
    private readonly IFarmRepository _farmRepository;
    private readonly ICropSeasonRepository _cropSeasonRepository;
    private readonly IPlantPositionRepository _plantPositionRepository;

    public PlantPositionValidator(
        IFarmRepository farmRepository, 
        ICropSeasonRepository cropSeasonRepository,
        IPlantPositionRepository plantPositionRepository)
    {
        _farmRepository = farmRepository;
        _cropSeasonRepository = cropSeasonRepository;
        _plantPositionRepository = plantPositionRepository;
    }

    public async Task ValidateAddPlantAsync(CreatePlantPositionDto dto)
    {
        var farm = await _farmRepository.GetByIdAsync(dto.FarmId);
        if (farm == null)
            throw new InvalidOperationException($"Không tìm thấy rẫy với ID: {dto.FarmId}");

        if (dto.SeasonId.HasValue)
        {
            var season = await _cropSeasonRepository.GetByIdAsync(dto.SeasonId.Value);
            if (season == null)
                throw new InvalidOperationException($"Không tìm thấy vụ mùa với ID: {dto.SeasonId}");
        }

        if (await _plantPositionRepository.PositionExistsAsync(dto.FarmId, dto.RowNumber, dto.ColumnNumber))
            throw new InvalidOperationException($"Vị trí hàng {dto.RowNumber}, cột {dto.ColumnNumber} đã có cây trong rẫy này");
    }

    public void ValidateUpdatePlant(PlantPosition? position, Guid id)
    {
        if (position == null)
            throw new KeyNotFoundException($"Không tìm thấy vị trí cây với ID: {id}");
    }

    public void ValidateRemovePlant(PlantPosition? position, Guid id)
    {
        if (position == null)
            throw new KeyNotFoundException($"Không tìm thấy vị trí cây với ID: {id}");
    }

    public async Task ValidateBulkCreatePlantsAsync(Guid farmId)
    {
        var farm = await _farmRepository.GetByIdAsync(farmId);
        if (farm == null)
            throw new InvalidOperationException($"Không tìm thấy rẫy với ID: {farmId}");
    }
}
