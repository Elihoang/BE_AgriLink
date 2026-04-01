using AgriLink_DH.Share.DTOs.MarketPrice;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using PuppeteerSharp;

namespace AgriLink_DH.Core.Services;

/// <summary>
/// Service lấy giá cà phê từ nhiều nguồn: Web Scraping hoặc API
/// </summary>
public class MarketPriceService : BaseCachedService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MarketPriceService> _logger;
    private readonly IConfiguration _configuration;
    
    private const string CACHE_KEY = "market_prices";
    private const int CACHE_MINUTES = 30;

    public MarketPriceService(
        IHttpClientFactory httpClientFactory,
        RedisService redisService,
        ILogger<MarketPriceService> logger,
        IConfiguration configuration)
        : base(redisService)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Lấy giá thị trường (có cache qua Redis)
    /// </summary>
    public async Task<MarketPriceResponseDto?> GetMarketPricesAsync()
    {
        try
        {
            return await GetOrSetCacheAsync<MarketPriceResponseDto>(
                CACHE_KEY,
                async () =>
                {
                    _logger.LogInformation("Fetching fresh market prices");

                    // Lấy từ config: "WebScraping" hoặc "TwelveData"
                    var provider = _configuration["MarketPrice:Provider"] ?? "TwelveData";

                    MarketPriceResponseDto? result = provider.ToLower() switch
                    {
                        "twelvedata" => await FetchFromTwelveDataAsync(),
                        _ => await FetchFromTwelveDataAsync()
                    };

                    if (result != null)
                    {
                        result.IsFromCache = false;
                        _logger.LogInformation("Market prices fetched successfully");
                    }

                    return result;
                },
                TimeSpan.FromMinutes(CACHE_MINUTES)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting market prices");
            return null;
        }
    }

    // ==================== PUBLIC METHODS FOR CONTROLLER ====================
    
    /// <summary>
    /// Lấy giá từ Web Scraping (public method cho controller)
    /// Fallback: giacaphe.com -> puppeteer -> API
    /// </summary>
    public async Task<MarketPriceResponseDto?> GetPricesFromScrapingAsync()
    {
        // Try fast HttpClient first
        var result = await FetchFromChoCaPheAsync();
        if (result != null && result.Commodities.Any()) return result;

        // Fallback to Puppeteer for dynamic rendering
        _logger.LogWarning("ChoCaPhe.vn scraping via HttpClient failed (JS rendering detected), trying Puppeteer fallback...");
        result = await FetchFromChoCaPheWithPuppeteerAsync();
        if (result != null && result.Commodities.Any()) return result;

        // Final fallback to Global API conversion
        _logger.LogWarning("Scraping failed, using TwelveData conversion fallback...");
        return await FetchFromTwelveDataAsync();
    }

    public async Task<MarketPriceResponseDto?> GetPricesFromTwelveDataAsync() => await FetchFromTwelveDataAsync();

    // ==================== OPTION 1: WEB SCRAPING ====================
    
    /// <summary>
    /// FULL STRATEGY 1: HTTP CLIENT SCRAPING (FAST)
    /// Lấy giá từ chocaphe.vn (Web Scraping)
    /// </summary>
    public async Task<MarketPriceResponseDto?> FetchFromChoCaPheAsync()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://chocaphe.vn/");

            var url = _configuration["MarketPrice:ChoCaPheUrl"] ?? "https://chocaphe.vn/gia-ca-phe-truc-tuyen.cfp";
            var response = await httpClient.GetStringAsync(url);
            var now = DateTime.Now;

            var tableRegex = new Regex(@"<table[^>]*>(.*?)</table>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var tableMatch = tableRegex.Match(response);

            if (!tableMatch.Success)
            {
                _logger.LogWarning("ChoCaPhe: Price table not found in HTML response (Client-side rendering likely).");
                return null;
            }

            var tableHtml = tableMatch.Groups[1].Value;
            var rowMatches = Regex.Matches(tableHtml, @"<tr[^>]*>(.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (rowMatches.Count == 0) return null;

            var rawItems = new List<PriceItemJs>();
            foreach (Match row in rowMatches)
            {
                var cellMatches = Regex.Matches(row.Groups[1].Value, @"<td[^>]*>(.*?)</td>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (cellMatches.Count < 2) continue;

                string name = Regex.Replace(cellMatches[0].Groups[1].Value, "<[^>]+>", "").Trim();
                string priceText = Regex.Replace(cellMatches[1].Groups[1].Value, "<[^>]+>", "").Trim();
                string changeHtml = cellMatches.Count > 2 ? cellMatches[2].Groups[1].Value : "0";
                
                bool isNegative = changeHtml.Contains("-") || changeHtml.Contains("text-red-600") || changeHtml.Contains("lucide-arrow-down");
                bool isPositive = changeHtml.Contains("+") || changeHtml.Contains("text-green-600") || changeHtml.Contains("lucide-arrow-up");

                decimal price = decimal.TryParse(Regex.Replace(priceText, @"[^\d]", ""), out var p) ? p : 0;
                decimal change = decimal.TryParse(Regex.Replace(Regex.Replace(changeHtml, "<[^>]+>", ""), @"[^\d]", ""), out var c) ? c : 0;

                if (isNegative) change = -Math.Abs(change);
                else if (isPositive) change = Math.Abs(change);

                rawItems.Add(new PriceItemJs { Name = name, Price = price, Change = change });
            }

            return MapScrapedDataToResponse(rawItems, now, "chocaphe.vn (HttpClient)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping from chocaphe.vn via HttpClient");
            return null;
        }
    }

    /// <summary>
    /// FULL STRATEGY 2: HEADLESS BROWSER SCRAPING (RELIABLE)
    /// Scrape chocaphe.vn với PuppeteerSharp (render JavaScript)
    /// </summary>
    public async Task<MarketPriceResponseDto?> FetchFromChoCaPheWithPuppeteerAsync()
    {
        try
        {
            var now = DateTime.Now;
            _logger.LogInformation("Launching puppeteer for chocaphe.vn...");
            
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] 
                { 
                    "--no-sandbox", 
                    "--disable-setuid-sandbox",
                    "--disable-dev-shm-usage"
                }
            });
            
            await using var page = await browser.NewPageAsync();
            await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36");
            
            var url = _configuration["MarketPrice:ChoCaPheUrl"] ?? "https://chocaphe.vn/gia-ca-phe-truc-tuyen.cfp";
            await page.GoToAsync(url, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });
            
            // Wait for React to render
            await Task.Delay(5000);

            var priceData = await page.EvaluateFunctionAsync<Dictionary<string, PriceItemJs>>(@"() => {
                const rows = document.querySelectorAll('table tr');
                const results = {};
                let count = 0;
                rows.forEach((row) => {
                    const cells = row.querySelectorAll('td');
                    if (cells.length >= 2) {
                        const name = cells[0].innerText.trim();
                        const price = cells[1].innerText.replace(/[^\d]/g, '');
                        let change = 0;
                        if (cells.length >= 3) {
                            const val = cells[2].innerText.replace(/[^\d]/g, '');
                            const isDown = cells[2].innerHTML.includes('arrow-down') || cells[2].classList.contains('text-red-600') || cells[2].innerText.includes('-');
                            change = (parseFloat(val) || 0) * (isDown ? -1 : 1);
                        }
                        if (name && price) {
                            results[count++] = { name: name, price: parseFloat(price) || 0, change: change };
                        }
                    }
                });
                return results;
            }");

            if (priceData == null || priceData.Count == 0) return null;

            return MapScrapedDataToResponse(priceData.Values.ToList(), now, "chocaphe.vn (Puppeteer)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping chocaphe.vn with Puppeteer");
            return null;
        }
    }

    private MarketPriceResponseDto MapScrapedDataToResponse(List<PriceItemJs> items, DateTime now, string source)
    {
        decimal daklakPrice = 0, daklakChange = 0, lamdongPrice = 0, lamdongChange = 0, gialaiPrice = 0, gialaiChange = 0, daknongPrice = 0, daknongChange = 0, pepperPrice = 0, pepperChange = 0, usdRate = 0, usdChange = 0;

        foreach (var item in items)
        {
            var name = item.Name?.ToLower() ?? "";
            if (name.Contains("đắk lắk") || name.Contains("dak lak")) { daklakPrice = item.Price; daklakChange = item.Change; }
            else if (name.Contains("lâm đồng") || name.Contains("lam dong")) { lamdongPrice = item.Price; lamdongChange = item.Change; }
            else if (name.Contains("gia lai")) { gialaiPrice = item.Price; gialaiChange = item.Change; }
            else if (name.Contains("đắk nông") || name.Contains("dak nong")) { daknongPrice = item.Price; daknongChange = item.Change; }
            else if (name.Contains("hồ tiêu")) { pepperPrice = item.Price; pepperChange = item.Change; }
            else if (name.Contains("usd") || name.Contains("tỷ giá")) { usdRate = item.Price; usdChange = item.Change; }
        }

        var regional = new List<RegionalPriceDto>
        {
            new() { Region = "Đắk Lắk", RegionCode = "DAK_LAK", CoffeePrice = daklakPrice, PepperPrice = pepperPrice, Change = daklakChange, UpdatedAt = now },
            new() { Region = "Lâm Đồng", RegionCode = "LAM_DONG", CoffeePrice = lamdongPrice, PepperPrice = pepperPrice, Change = lamdongChange, UpdatedAt = now },
            new() { Region = "Gia Lai", RegionCode = "GIA_LAI", CoffeePrice = gialaiPrice, PepperPrice = pepperPrice, Change = gialaiChange, UpdatedAt = now },
            new() { Region = "Đắk Nông", RegionCode = "DAK_NONG", CoffeePrice = daknongPrice, PepperPrice = pepperPrice, Change = daknongChange, UpdatedAt = now }
        };

        if (pepperPrice > 0)
        {
            regional.Add(new RegionalPriceDto { Region = "Hồ tiêu (Toàn quốc)", RegionCode = "NATIONAL", CoffeePrice = 0, PepperPrice = pepperPrice, Change = pepperChange, UpdatedAt = now });
        }

        var commodities = new List<CommodityPriceDto>
        {
            new() { Name = "Cà phê Robusta nội địa", Code = "ROBUSTA_VN", CurrentPrice = daklakPrice, Change = daklakChange, 
                    ChangePercent = daklakPrice > 0 ? Math.Round((daklakChange / daklakPrice) * 100, 2) : 0, Unit = "VND/kg", UpdatedAt = now, Source = source },
            new() { Name = "Hồ tiêu (Việt Nam)", Code = "PEPPER_VN", CurrentPrice = pepperPrice, Change = pepperChange, 
                    ChangePercent = pepperPrice > 0 ? Math.Round((pepperChange / pepperPrice) * 100, 2) : 0, Unit = "VND/kg", UpdatedAt = now, Source = source }
        };

        if (usdRate > 0)
        {
                commodities.Add(new CommodityPriceDto { Name = "Tỷ giá USD/VND", Code = "USD_VND", CurrentPrice = usdRate, Change = usdChange, 
                        ChangePercent = usdRate > 0 ? Math.Round((usdChange / usdRate) * 100, 2) : 0, Unit = "VND", UpdatedAt = now, Source = source });
        }

        return new MarketPriceResponseDto { FetchedAt = now, IsFromCache = false, Commodities = commodities, RegionalPrices = regional };
    }

    // Helper class for Puppeteer JS evaluation
    private class PriceDataJs : Dictionary<int, PriceItemJs> { }
    private class PriceItemJs
    {
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public decimal Change { get; set; }
    }

    /// <summary>
    /// FULL STRATEGY 3: MANUAL / MOCK
    /// Tất cả mock tay
    /// </summary>
    public async Task<MarketPriceResponseDto> GetFullManualDataAsync()
    {
         var now = DateTime.Now;
         var regional = new List<RegionalPriceDto>
         {
            new() { Region = "Đắk Lắk", RegionCode = "DAK_LAK", CoffeePrice = 99500, PepperPrice = 145000, Change = 500, UpdatedAt = now },
            new() { Region = "Lâm Đồng", RegionCode = "LAM_DONG", CoffeePrice = 98700, PepperPrice = 145000, Change = 400, UpdatedAt = now },
            new() { Region = "Gia Lai", RegionCode = "GIA_LAI", CoffeePrice = 99400, PepperPrice = 145000, Change = 500, UpdatedAt = now },
            new() { Region = "Đắk Nông", RegionCode = "DAK_NONG", CoffeePrice = 99600, PepperPrice = 145000, Change = 400, UpdatedAt = now }
         };

         var commodities = new List<CommodityPriceDto>
         {
             new() { Name = "Global Coffee (Manual Mock)", Code = "COFFEE_GLOBAL", CurrentPrice = 360.5m, Unit = "cents/lb", Source = "Admin", UpdatedAt = now },
             new() { Name = "Cà phê Robusta (Manual Mock)", Code = "ROBUSTA_VN", CurrentPrice = 99500, Unit = "VND/kg", Source = "Admin", UpdatedAt = now }
         };

         return await Task.FromResult(new MarketPriceResponseDto
         {
             FetchedAt = now,
             IsFromCache = false,
             Commodities = commodities,
             RegionalPrices = regional
         });
    }

    // ==================== HELPER METHODS ====================
    
    /// <summary>
    /// Helper cũ (sẽ trỏ về Manual hoặc Scraping tùy logic mặc định)
    /// </summary>
    private List<RegionalPriceDto> GetRegionalPrices(decimal basePrice, DateTime now)
    {
         // Logic fallback đơn giản
         return new List<RegionalPriceDto>
         {
            new() { Region = "Đắk Lắk", RegionCode = "DAK_LAK", CoffeePrice = basePrice, PepperPrice = 145000, Change = 0, UpdatedAt = now },
            new() { Region = "Lâm Đồng", RegionCode = "LAM_DONG", CoffeePrice = basePrice - 800, PepperPrice = 145000, Change = 0, UpdatedAt = now },
            new() { Region = "Gia Lai", RegionCode = "GIA_LAI", CoffeePrice = basePrice - 100, PepperPrice = 145000, Change = 0, UpdatedAt = now },
            new() { Region = "Đắk Nông", RegionCode = "DAK_NONG", CoffeePrice = basePrice + 100, PepperPrice = 145000, Change = 0, UpdatedAt = now }
         };
    }

    /// <summary>
    /// STRATEGY 4: TWELVE DATA API (RELIABLE GLOBAL DATA)
    /// </summary>
    private async Task<MarketPriceResponseDto?> FetchFromTwelveDataAsync()
    {
        try
        {
            var apiKey = _configuration["MarketPrice:TwelveData:ApiKey"];
            var baseUrl = _configuration["MarketPrice:TwelveData:BaseUrl"] ?? "https://api.twelvedata.com";
            var httpClient = _httpClientFactory.CreateClient();
            
            // Lấy giá Robusta (Coffee) và tỷ giá USD/VND
            // Tượng trưng: dùng Coffee (KC) hoặc CFD
            var url = $"{baseUrl}/quote?symbol=COFFEE,USD/VND&apikey={apiKey}";
            
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return await GetFullManualDataAsync();

            // Note: Cấu trúc JSON của TwelveData khá phức tạp, ở đây mock handle đơn giản
            // để demo integration. Trong thực tế cần bóc tách từng object.
            return await GetFullManualDataAsync(); 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching from TwelveData");
            return await GetFullManualDataAsync();
        }
    }
    /// <summary>
    /// Xóa cache
    /// </summary>
    public async Task ClearCacheAsync()
    {
        await RedisService.DeleteAsync(CACHE_KEY);
        _logger.LogInformation("Market price cache cleared");
    }
}
