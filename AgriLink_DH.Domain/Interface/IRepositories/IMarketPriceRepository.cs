using AgriLink_DH.Domain.Models;
using System.Linq.Expressions;

namespace AgriLink_DH.Domain.Interface.IRepositories;

public interface IMarketPriceRepository : IRepository<MarketPriceHistory>
{
    Task<IEnumerable<MarketPriceHistory>> GetLatestPricesAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<DateTime?> GetLastRecordedDateAsync(CancellationToken cancellationToken = default);
    Task<MarketPriceHistory?> GetPriceAsync(Guid productId, string? regionCode, DateTime date, CancellationToken cancellationToken = default);
    Task<decimal?> GetPreviousPriceAsync(Guid productId, string? regionCode, DateTime beforeDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<MarketPriceHistory>> GetHistoryAsync(Guid productId, string? regionCode, DateTime? fromDate, DateTime? toDate, int limit, CancellationToken cancellationToken = default);
    Task<IEnumerable<MarketPriceHistory>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);
}
