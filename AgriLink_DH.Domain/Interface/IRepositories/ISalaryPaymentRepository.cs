using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

public interface ISalaryPaymentRepository : IRepository<SalaryPayment>
{
    Task<IEnumerable<SalaryPayment>> GetByWorkerIdAsync(Guid workerId);
    Task<IEnumerable<SalaryPayment>> GetByPeriodAsync(DateTime start, DateTime end);
}
