using AgriLink_DH.Infrastructure.Data;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Infrastructure.Repositories;

public class TaskTypeRepository : BaseRepository<TaskType>, ITaskTypeRepository
{
    public TaskTypeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TaskType>> GetByFarmIdAsync(Guid farmId)
    {
        return await _dbSet
            .Where(t => t.FarmId == farmId)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }
}

