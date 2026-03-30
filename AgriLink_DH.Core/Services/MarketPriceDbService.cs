using AgriLink_DH.Share.DTOs.MarketPrice;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using Microsoft.Extensions.Logging;

namespace AgriLink_DH.Core.Services;

/// <summary>
/// Service quản lý giá thị trường - Lưu vào Database
/// </summary>
public class MarketPriceDbService
{
    private readonly IMarketPriceRepository _marketPriceRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarketPriceDbService> _logger;

    public MarketPriceDbService(
        IMarketPriceRepository marketPriceRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ILogger<MarketPriceDbService> logger)
    {
        _marketPriceRepository = marketPriceRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
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
            
            // Lấy tất cả giá mới nhất
            var latestPrices = (await _marketPriceRepository.GetLatestPricesAsync(today)).ToList();
            
            // Nếu hôm nay chưa có data, lấy ngày gần nhất
            if (!latestPrices.Any())
            {
                var lastDate = await _marketPriceRepository.GetLastRecordedDateAsync();
                
                if (lastDate.HasValue)
                {
                    latestPrices = (await _marketPriceRepository.GetLatestPricesAsync(lastDate.Value)).ToList();
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
    /// Admin cập nhật giá thủ công (hoặc từ Excel import).
    /// Change & ChangePercent được tự tính từ DB — không phụ thuộc request.Change.
    /// </summary>
    public async Task<MarketPriceHistory> UpdatePriceAsync(UpdateMarketPriceRequest request, string? updatedBy = null)
    {
        try
        {
            var recordedDate = request.RecordedDate ?? DateTime.Today;

            // ── Tự tính Change từ giá ngày gần nhất trước recordedDate ──────────
            var prevPrice = await _marketPriceRepository.GetPreviousPriceAsync(request.ProductId, request.RegionCode, recordedDate);
            var autoChange        = prevPrice.HasValue ? request.Price - prevPrice.Value : 0m;
            var autoChangePercent = prevPrice.HasValue && prevPrice.Value > 0
                                    ? (autoChange / prevPrice.Value) * 100m
                                    : 0m;

            // ── Check record hôm nay đã tồn tại chưa ────────────────────────────
            var existing = await _marketPriceRepository.GetPriceAsync(request.ProductId, request.RegionCode, recordedDate);

            if (existing != null)
            {
                // Update — ghi đè giá, tính lại Change
                existing.Price         = request.Price;
                existing.Change        = autoChange;
                existing.ChangePercent = autoChangePercent;
                existing.Source        = request.Source;
                existing.UpdatedBy     = updatedBy ?? "Admin";
                existing.Notes         = $"Updated at {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                _marketPriceRepository.Update(existing);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Updated price: ProductID={ProductId} Region={Region} Date={Date} Change={Change}",
                    request.ProductId, request.RegionCode, recordedDate, autoChange);

                return existing;
            }
            else
            {
                // Create new
                var newRecord = new MarketPriceHistory
                {
                    ProductId     = request.ProductId,
                    Region        = request.Region,
                    RegionCode    = request.RegionCode,
                    Price         = request.Price,
                    Change        = autoChange,
                    ChangePercent = autoChangePercent,
                    Unit          = "VND/kg",
                    RecordedDate  = recordedDate,
                    Source        = request.Source ?? "Admin",
                    UpdatedBy     = updatedBy ?? "Admin",
                    CreatedAt     = DateTime.UtcNow,
                    Notes         = "Manual entry"
                };

                await _marketPriceRepository.AddAsync(newRecord);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Created price: ProductID={ProductId} Region={Region} Date={Date} Change={Change}",
                    request.ProductId, request.RegionCode, recordedDate, autoChange);

                return newRecord;
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
            var results = await _marketPriceRepository.GetHistoryAsync(productId, regionCode, fromDate, toDate, limit);
            return results.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price history");
            throw;
        }
    }

    /// <summary>
    /// Lấy giá hôm trước (để tính toán Change tự động)
    /// </summary>
    public async Task<Dictionary<string, decimal>> GetPreviousDayPricesAsync()
    {
        try
        {
            var today = DateTime.Today;
            
            // Tìm ngày gần nhất trước hôm nay
            var lastDate = await _marketPriceRepository.GetLastRecordedDateAsync();
            
            if (!lastDate.HasValue)
            {
                return new Dictionary<string, decimal>();
            }
            
            // Lấy tất cả giá của ngày đó
            var previousPrices = await _marketPriceRepository.GetByDateAsync(lastDate.Value);
            
            // Tạo dictionary với key: "productId|regionCode"
            var priceDict = new Dictionary<string, decimal>();
            
            foreach (var price in previousPrices)
            {
                var key = $"{price.ProductId}|{price.RegionCode ?? "NATIONAL"}";
                priceDict[key] = price.Price;
            }
            
            return priceDict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting previous day prices");
            throw;
        }
    }

    /// <summary>
    /// Lấy ProductId từ Code sản phẩm (VD: "CF_ROBUSTA", "PEPPER").
    /// Dùng cho Excel import — tránh hardcode GUID.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Khi không tìm thấy sản phẩm với code đó.</exception>
    public async Task<Guid> GetProductIdByCodeAsync(string code)
    {
        var product = await _productRepository.GetByCodeAsync(code);

        if (product == null)
            throw new KeyNotFoundException($"Không tìm thấy sản phẩm với code: '{code}'.");

        return product.Id;
    }
}
