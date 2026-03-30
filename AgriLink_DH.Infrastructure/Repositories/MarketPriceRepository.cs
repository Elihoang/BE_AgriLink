using AgriLink_DH.Domain.Models;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Infrastructure.Repositories;

public class MarketPriceRepository : BaseRepository<MarketPriceHistory>, IMarketPriceRepository
{
    public MarketPriceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MarketPriceHistory>> GetLatestPricesAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(mph => mph.Product)
            .Where(mph => mph.RecordedDate == date)
            .ToListAsync(cancellationToken);
    }

    public async Task<DateTime?> GetLastRecordedDateAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.MaxAsync(mph => (DateTime?)mph.RecordedDate, cancellationToken);
    }

    public async Task<MarketPriceHistory?> GetPriceAsync(Guid productId, string? regionCode, DateTime date, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(mph => 
                mph.ProductId == productId && 
                mph.RegionCode == regionCode && 
                mph.RecordedDate == date, cancellationToken);
    }

    public async Task<decimal?> GetPreviousPriceAsync(Guid productId, string? regionCode, DateTime beforeDate, CancellationToken cancellationToken = default)
    {
        var prevDate = await _dbSet
            .Where(m => m.ProductId == productId && m.RegionCode == regionCode && m.RecordedDate < beforeDate)
            .MaxAsync(m => (DateTime?)m.RecordedDate, cancellationToken);

        if (prevDate == null) return null;

        return await _dbSet
            .Where(m => m.ProductId == productId && m.RegionCode == regionCode && m.RecordedDate == prevDate.Value)
            .Select(m => (decimal?)m.Price)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<MarketPriceHistory>> GetHistoryAsync(Guid productId, string? regionCode, DateTime? fromDate, DateTime? toDate, int limit, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Include(mph => mph.Product).Where(mph => mph.ProductId == productId);
        
        if (!string.IsNullOrEmpty(regionCode))
            query = query.Where(mph => mph.RegionCode == regionCode);
        
        if (fromDate.HasValue)
            query = query.Where(mph => mph.RecordedDate >= fromDate.Value);
        
        if (toDate.HasValue)
            query = query.Where(mph => mph.RecordedDate <= toDate.Value);
        
        return await query
            .OrderByDescending(mph => mph.RecordedDate)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MarketPriceHistory>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(mph => mph.Product)
            .Where(mph => mph.RecordedDate == date)
            .ToListAsync(cancellationToken);
    }
}
