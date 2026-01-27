using AgriLink_DH.Share.DTOs.MarketPrice;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Core.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Core.Services;

/// <summary>
/// Service quản lý giá thị trường - Lưu vào Database
/// </summary>
public class MarketPriceDbService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MarketPriceDbService> _logger;

    public MarketPriceDbService(
        ApplicationDbContext context,
        ILogger<MarketPriceDbService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Lấy giá mới nhất (hôm nay hoặc gần nhất)
    /// </summary>
    public async Task<MarketPriceResponseDto> GetLatestPricesAsync()
    {
        try
        {
            var today = DateTime.Today;
            
            // Lấy tất cả giá mới nhất, Include Product để lấy tên/code
            var latestPrices = await _context.MarketPriceHistory
                .Include(mph => mph.Product)
                .Where(mph => mph.RecordedDate == today)
                .ToListAsync();
            
            // Nếu hôm nay chưa có data, lấy ngày gần nhất
            if (!latestPrices.Any())
            {
                var lastDate = await _context.MarketPriceHistory
                    .MaxAsync(mph => (DateTime?)mph.RecordedDate);
                
                if (lastDate.HasValue)
                {
                    latestPrices = await _context.MarketPriceHistory
                        .Include(mph => mph.Product)
                        .Where(mph => mph.RecordedDate == lastDate.Value)
                        .ToListAsync();
                }
            }
            
            // Group data dựa trên Product Code
            var coffeeData = latestPrices.Where(p => p.Product?.Code == "CF_ROBUSTA").ToList();
            var pepperData = latestPrices.Where(p => p.Product?.Code == "PEPPER").ToList();
            
            // Build response
            var commodities = new List<CommodityPriceDto>();
            
            if (coffeeData.Any())
            {
                var avgCoffee = coffeeData.Average(c => c.Price);
                var avgChange = coffeeData.Average(c => c.Change);
                var product = coffeeData.First().Product;
                
                commodities.Add(new CommodityPriceDto
                {
                    Name = $"{product?.Name} (Trung bình)",
                    Code = "COFFEE_AVG",
                    CurrentPrice = Math.Round(avgCoffee, 0),
                    Change = Math.Round(avgChange, 0),
                    ChangePercent = avgCoffee > 0 ? Math.Round((avgChange / avgCoffee) * 100, 2) : 0,
                    Unit = "VND/kg",
                    UpdatedAt = coffeeData.First().RecordedDate,
                    Source = "Database (Admin)"
                });
            }
            
            if (pepperData.Any())
            {
                var avgPepper = pepperData.Average(p => p.Price);
                var avgChange = pepperData.Average(p => p.Change);
                var product = pepperData.First().Product;
                
                commodities.Add(new CommodityPriceDto
                {
                    Name = $"{product?.Name} (Trung bình)",
                    Code = "PEPPER_AVG",
                    CurrentPrice = Math.Round(avgPepper, 0),
                    Change = Math.Round(avgChange, 0),
                    ChangePercent = avgPepper > 0 ? Math.Round((avgChange / avgPepper) * 100, 2) : 0,
                    Unit = "VND/kg",
                    UpdatedAt = pepperData.First().RecordedDate,
                    Source = "Database (Admin)"
                });
            }
            
            // Regional prices (Coffee only for now)
            var regionalPrices = coffeeData.Select(c => new RegionalPriceDto
            {
                Region = c.Region ?? "N/A",
                RegionCode = c.RegionCode ?? "N/A",
                CoffeePrice = c.Price,
                PepperPrice = pepperData.FirstOrDefault(p => p.RegionCode == c.RegionCode)?.Price ?? 0,
                Change = c.Change,
                UpdatedAt = c.RecordedDate
            }).ToList();
            
            return new MarketPriceResponseDto
            {
                FetchedAt = DateTime.Now,
                IsFromCache = false,
                Commodities = commodities,
                RegionalPrices = regionalPrices
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest prices from database");
            throw;
        }
    }

    /// <summary>
    /// Admin cập nhật giá thủ công
    /// </summary>
    public async Task<MarketPriceHistory> UpdatePriceAsync(UpdateMarketPriceRequest request, string? updatedBy = null)
    {
        try
        {
            var recordedDate = request.RecordedDate ?? DateTime.Today;
            
            // Check if already exists
            var existing = await _context.MarketPriceHistory
                .FirstOrDefaultAsync(mph => 
                    mph.ProductId == request.ProductId &&
                    mph.RegionCode == request.RegionCode &&
                    mph.RecordedDate == recordedDate);
            
            if (existing != null)
            {
                // Update
                existing.Price = request.Price;
                existing.Change = request.Change;
                existing.ChangePercent = existing.Price > 0 ? (request.Change / existing.Price) * 100 : 0;
                existing.Source = request.Source;
                existing.UpdatedBy = updatedBy ?? "Admin";
                existing.Notes = $"Updated at {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Updated price for ProductID {request.ProductId} - {request.RegionCode}");
                
                return existing;
            }
            else
            {
                // Create new
                var newPrice = new MarketPriceHistory
                {
                    ProductId = request.ProductId,
                    Region = request.Region,
                    RegionCode = request.RegionCode,
                    Price = request.Price,
                    Change = request.Change,
                    ChangePercent = request.Price > 0 ? (request.Change / request.Price) * 100 : 0,
                    Unit = "VND/kg",
                    RecordedDate = recordedDate,
                    Source = request.Source ?? "Admin",
                    UpdatedBy = updatedBy ?? "Admin",
                    CreatedAt = DateTime.UtcNow,
                    Notes = "Manual entry"
                };
                
                _context.MarketPriceHistory.Add(newPrice);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Created new price for ProductID {request.ProductId} - {request.RegionCode}");
                
                return newPrice;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating price");
            throw;
        }
    }

    /// <summary>
    /// Lấy lịch sử giá theo ProductId và Region
    /// </summary>
    public async Task<List<MarketPriceHistory>> GetPriceHistoryAsync(
        Guid productId,
        string? regionCode = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int limit = 30)
    {
        try
        {
            var query = _context.MarketPriceHistory
                .Include(mph => mph.Product)
                .Where(mph => mph.ProductId == productId);
            
            if (!string.IsNullOrEmpty(regionCode))
            {
                query = query.Where(mph => mph.RegionCode == regionCode);
            }
            
            if (fromDate.HasValue)
            {
                query = query.Where(mph => mph.RecordedDate >= fromDate.Value);
            }
            
            if (toDate.HasValue)
            {
                query = query.Where(mph => mph.RecordedDate <= toDate.Value);
            }
            
            return await query
                .OrderByDescending(mph => mph.RecordedDate)
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price history");
            throw;
        }
    }
}
