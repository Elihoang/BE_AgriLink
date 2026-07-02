using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Core.Validations;

public class WeatherLogValidator
{
    private readonly IFarmRepository _farmRepository;

    public WeatherLogValidator(IFarmRepository farmRepository)
    {
        _farmRepository = farmRepository;
    }

    public async Task ValidateCreateLogAsync(Guid farmId)
    {
        var farm = await _farmRepository.GetByIdAsync(farmId);
        if (farm == null)
            throw new InvalidOperationException($"Không tìm thấy vườn với ID: {farmId}");
    }

    public void ValidateDeleteLog(WeatherLog? log, Guid id)
    {
        if (log == null)
            throw new KeyNotFoundException($"Không tìm thấy nhật ký thời tiết với ID: {id}");
    }
}
