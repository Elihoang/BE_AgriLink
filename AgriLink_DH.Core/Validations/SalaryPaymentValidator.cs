using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Core.Validations;

public class SalaryPaymentValidator
{
    private readonly IWorkerRepository _workerRepository;

    public SalaryPaymentValidator(IWorkerRepository workerRepository)
    {
        _workerRepository = workerRepository;
    }

    public async Task ValidateCalculateSalaryAsync(Guid workerId)
    {
        var worker = await _workerRepository.GetByIdAsync(workerId);
        if (worker == null) throw new KeyNotFoundException("Không tìm thấy nhân công.");
    }

    public async Task ValidateExecutePaymentAsync(Guid workerId)
    {
        var worker = await _workerRepository.GetByIdAsync(workerId);
        if (worker == null) throw new KeyNotFoundException("Không tìm thấy nhân công.");
    }
}
