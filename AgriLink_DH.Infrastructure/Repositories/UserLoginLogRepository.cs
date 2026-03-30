using AgriLink_DH.Infrastructure.Data;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Infrastructure.Repositories;

public class UserLoginLogRepository : BaseRepository<UserLoginLog>, IUserLoginLogRepository
{
    public UserLoginLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UserLoginLog>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(log => log.UserId == userId)
            .OrderByDescending(log => log.LoginTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserLoginLog>> GetRecentLoginsAsync(Guid userId, int count = 10)
    {
        return await _dbSet
            .Where(log => log.UserId == userId && log.IsSuccess)
            .OrderByDescending(log => log.LoginTime)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserLoginLog>> GetFailedLoginsAsync(Guid userId, DateTime since)
    {
        return await _dbSet
            .Where(log => log.UserId == userId && !log.IsSuccess && log.LoginTime >= since)
            .OrderByDescending(log => log.LoginTime)
            .ToListAsync();
    }
}

