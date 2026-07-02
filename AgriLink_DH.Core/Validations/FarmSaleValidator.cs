using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Core.Validations;

public class FarmSaleValidator
{
    private readonly ICropSeasonRepository _cropSeasonRepository;

    public FarmSaleValidator(ICropSeasonRepository cropSeasonRepository)
    {
        _cropSeasonRepository = cropSeasonRepository;
    }

    public async Task ValidateCreateSaleAsync(Guid seasonId)
    {
        var season = await _cropSeasonRepository.GetByIdAsync(seasonId);
        if (season == null)
            throw new InvalidOperationException($"Không tìm thấy vụ mùa với ID: {seasonId}");
    }

    public void ValidateDeleteSale(FarmSale? sale, Guid id)
    {
        if (sale == null)
            throw new KeyNotFoundException($"Không tìm thấy phiếu bán hàng với ID: {id}");
    }
}
