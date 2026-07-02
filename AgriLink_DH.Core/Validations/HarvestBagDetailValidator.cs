using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Core.Validations;

public class HarvestBagDetailValidator
{
    private readonly IHarvestSessionRepository _harvestSessionRepository;

    public HarvestBagDetailValidator(IHarvestSessionRepository harvestSessionRepository)
    {
        _harvestSessionRepository = harvestSessionRepository;
    }

    public async Task ValidateAddBagAsync(Guid sessionId)
    {
        var session = await _harvestSessionRepository.GetWithDetailsAsync(sessionId);
        if (session == null)
            throw new InvalidOperationException($"Không tìm thấy phiếu thu hoạch với ID: {sessionId}");
    }

    public async Task ValidateAddDraftBagAsync(Guid sessionId)
    {
        var session = await _harvestSessionRepository.GetByIdAsync(sessionId);
        if (session == null)
            throw new InvalidOperationException($"Không tìm thấy phiếu thu hoạch với ID: {sessionId}");
    }

    public async Task ValidateConfirmDraftsAsync(Guid sessionId)
    {
        var session = await _harvestSessionRepository.GetWithDetailsAsync(sessionId);
        if (session == null)
            throw new InvalidOperationException($"Không tìm thấy phiếu thu hoạch với ID: {sessionId}");
    }

    public void ValidateDeleteBag(HarvestBagDetail? bag, Guid id)
    {
        if (bag == null)
            throw new KeyNotFoundException($"Không tìm thấy bao với ID: {id}");
    }
}
