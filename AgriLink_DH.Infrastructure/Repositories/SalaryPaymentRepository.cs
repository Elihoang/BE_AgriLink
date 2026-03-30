using AgriLink_DH.Infrastructure.Data;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Infrastructure.Repositories;

public class SalaryPaymentRepository : BaseRepository<SalaryPayment>, ISalaryPaymentRepository
{
    public SalaryPaymentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SalaryPayment>> GetByWorkerIdAsync(Guid workerId)
    {
        return await _dbSet.Include(sp => sp.Worker)
                           .Where(sp => sp.WorkerId == workerId)
                           .OrderByDescending(sp => sp.CreatedAt)
                           .ToListAsync();
    }

    public async Task<IEnumerable<SalaryPayment>> GetByPeriodAsync(DateTime start, DateTime end)
    {
        return await _dbSet.Include(sp => sp.Worker)
                           .Where(sp => sp.PeriodStart >= start && sp.PeriodEnd <= end)
                           .OrderByDescending(sp => sp.CreatedAt)
                           .ToListAsync();
    }

    public override async Task<IEnumerable<SalaryPayment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(sp => sp.Worker)
                           .OrderByDescending(sp => sp.CreatedAt)
                           .ToListAsync(cancellationToken);
    }
}

