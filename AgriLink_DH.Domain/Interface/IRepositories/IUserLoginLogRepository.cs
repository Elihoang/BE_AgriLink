using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

public interface IUserLoginLogRepository : IRepository<UserLoginLog>
{
    Task<IEnumerable<UserLoginLog>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<UserLoginLog>> GetRecentLoginsAsync(Guid userId, int count = 10);
    Task<IEnumerable<UserLoginLog>> GetFailedLoginsAsync(Guid userId, DateTime since);
}
