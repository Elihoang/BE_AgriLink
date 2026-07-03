using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.CropSeason;

namespace AgriLink_DH.Core.Validations;

public class CropSeasonValidator
{
    private readonly ICropSeasonRepository _cropSeasonRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IProductRepository _productRepository;

    public CropSeasonValidator(
        ICropSeasonRepository cropSeasonRepository,
        IFarmRepository farmRepository,
        IProductRepository productRepository)
    {
        _cropSeasonRepository = cropSeasonRepository;
        _farmRepository = farmRepository;
        _productRepository = productRepository;
    }

    public async Task ValidateCreateAsync(CreateCropSeasonDto dto)
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
    }

    public void ValidateUpdate(CropSeason? season, Guid id)
    {
        if (season == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy vụ mùa với ID: {id}");
        }
    }

    public void ValidateDelete(CropSeason? season, Guid id)
    {
        if (season == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy vụ mùa với ID: {id}");
        }
    }

    public void ValidateUpdateStage(CropSeason? season, Guid id)
    {
        if (season == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy vụ mùa với ID: {id}");
        }
    }
}
