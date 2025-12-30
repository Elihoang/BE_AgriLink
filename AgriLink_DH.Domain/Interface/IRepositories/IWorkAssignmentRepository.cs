using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

public interface IWorkAssignmentRepository : IRepository<WorkAssignment>
{
    Task<IEnumerable<WorkAssignment>> GetByLogIdAsync(Guid logId);
    Task<IEnumerable<WorkAssignment>> GetByWorkerIdAsync(Guid workerId, DateTime fromDate, DateTime toDate);
}
