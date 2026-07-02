using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.HarvestSession;

namespace AgriLink_DH.Core.Validations;

public class HarvestSessionValidator
{
    private readonly ICropSeasonRepository _cropSeasonRepository;

    public HarvestSessionValidator(ICropSeasonRepository cropSeasonRepository)
    {
        _cropSeasonRepository = cropSeasonRepository;
    }

    public async Task ValidateCreateAsync(Guid seasonId)
    {
        var season = await _cropSeasonRepository.GetSeasonWithDetailsAsync(seasonId);
        if (season == null)
        {
            throw new InvalidOperationException($"Không tìm thấy vụ mùa với ID: {seasonId}");
        }
    }

    public void ValidateDelete(HarvestSession? session, Guid id)
    {
        if (session == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy phiếu thu hoạch với ID: {id}");
        }
    }
}
