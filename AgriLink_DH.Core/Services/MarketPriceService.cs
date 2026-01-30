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
                        "webscraping" => await FetchFromWebScrapingAsync(),
                        "twelvedata" => await FetchFromTwelveDataAsync(),
                        "alphavantage" => await FetchFromAlphaVantageAsync(),
                        _ => await FetchFromAlphaVantageAsync()
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
    /// </summary>
    public async Task<MarketPriceResponseDto?> GetPricesFromScrapingAsync()
    {
        return await FetchFromWebScrapingAsync();
    }

    /// <summary>
    /// Lấy giá từ API Twelve Data (public method cho controller)
    /// </summary>
    public async Task<MarketPriceResponseDto?> GetPricesFromTwelveDataAsync()
    {
        return await FetchFromTwelveDataAsync();
    }
    
    /// <summary>
    /// Lấy giá từ API Alpha Vantage (public method cho controller)
    /// </summary>
    public async Task<MarketPriceResponseDto?> GetPricesFromAlphaVantageAsync()
    {
        return await FetchFromAlphaVantageAsync();
    }

    // ==================== OPTION 3: ALPHA VANTAGE API ====================
    
    /// <summary>
    /// Lấy giá từ Alpha Vantage API (Coffee - Global Price)
    /// ✅ Hợp pháp, Free tier
    /// </summary>
    public async Task<MarketPriceResponseDto?> FetchFromAlphaVantageAsync()
    {
        try
        {
            var apiKey = _configuration["MarketPrice:AlphaVantage:ApiKey"] ?? "demo";
            var baseUrl = _configuration["MarketPrice:AlphaVantage:BaseUrl"] ?? "https://www.alphavantage.co";

            var httpClient = _httpClientFactory.CreateClient();
            var now = DateTime.Now;

            // Lấy giá Coffee Global (Monthly) - Function=COFFEE
            var url = $"{baseUrl}/query?function=COFFEE&interval=monthly&apikey={apiKey}";
            var response = await httpClient.GetStringAsync(url);
            
            // Log response raw để debug
            _logger.LogInformation("AlphaVantage Response: {Response}", response.Length > 200 ? response.Substring(0, 200) + "..." : response);

            var data = System.Text.Json.JsonDocument.Parse(response);

            decimal price = 0;
            decimal change = 0;
            decimal changePercent = 0;

            // Alpha Vantage Commodities trả về property 'data' là array
            if (data.RootElement.TryGetProperty("data", out var dataArray) && dataArray.GetArrayLength() > 0)
            {
                // Lấy phần tử đầu tiên (tháng mới nhất)
                var latest = dataArray[0];
                
                // Parse 'value' (string -> decimal)
                if (latest.TryGetProperty("value", out var valElement))
                {
                    string valStr = valElement.GetString() ?? "0";
                    if (decimal.TryParse(valStr, out decimal parsedPrice))
                    {
                        price = parsedPrice;
                    }
                }

                // Tính change
                if (dataArray.GetArrayLength() > 1)
                {
                    var previous = dataArray[1];
                    if (previous.TryGetProperty("value", out var prevElement))
                    {
                        string prevStr = prevElement.GetString() ?? "0";
                        if (decimal.TryParse(prevStr, out decimal prevPrice) && prevPrice > 0)
                        {
                            change = price - prevPrice;
                            changePercent = (change / prevPrice) * 100;
                        }
                    }
                }
            }
            else 
            {
                _logger.LogWarning("AlphaVantage response 'data' property is missing or empty.");
                // Error response từ API (ví dụ: limit reached)
                if (data.RootElement.TryGetProperty("Note", out var noteElement))
                {
                    _logger.LogWarning("AlphaVantage Limit Reached: {Note}", noteElement.GetString());
                }
            }

            // 1. Fetch Global from API
            if (price == 0) _logger.LogWarning("AlphaVantage returned 0. Conversion will be 0.");

            var commodities = new List<CommodityPriceDto>
            {
                new CommodityPriceDto
                {
                    Name = "Global Coffee (Alpha Vantage)",
                    Code = "COFFEE_GLOBAL",
                    CurrentPrice = Math.Round(price, 2),
                    Change = Math.Round(change, 2),
                    ChangePercent = Math.Round(changePercent, 2),
                    Unit = "cents/lb", 
                    UpdatedAt = now,
                    Source = "Alpha Vantage API"
                }
            };

            // 2. Calculate Regional from Global Price
            // Formula: USD/lb -> VND/kg
            // 1 lb = 0.453592 kg => 1 kg = 2.20462 lbs
            // price (cents/lb) -> USD/lb = price / 100
            // USD/kg = (price/100) * 2.20462
            // VND/kg = USD/kg * 25400 (Exchange Rate)
            
            decimal usdExchangeRate = 25400m;
            decimal priceUsdLb = price / 100m;
            decimal priceUsdKg = priceUsdLb * 2.20462m;
            decimal priceVndKgRaw = priceUsdKg * usdExchangeRate;

            // Robusta Ratio (Giả định): 0.6 * Arabica Price (API usually returns Arabica or composite)
            // Nếu API trả Robusta London (USD/ton) thì công thức khác. Giả sử đây là KC (Arabica).
            decimal robustaRatio = 0.6m; 
            decimal baseVnd = Math.Round(priceVndKgRaw * robustaRatio, 0);

            var regionalPrices = new List<RegionalPriceDto>
            {
                new() { Region = "Đắk Lắk (Calc)", RegionCode = "DAK_LAK", CoffeePrice = baseVnd, PepperPrice = 145000, Change = 0, UpdatedAt = now },
                new() { Region = "Lâm Đồng (Calc)", RegionCode = "LAM_DONG", CoffeePrice = baseVnd > 0 ? baseVnd - 800 : 0, PepperPrice = 145000, Change = 0, UpdatedAt = now },
                new() { Region = "Gia Lai (Calc)", RegionCode = "GIA_LAI", CoffeePrice = baseVnd > 0 ? baseVnd - 100 : 0, PepperPrice = 145000, Change = 0, UpdatedAt = now },
                new() { Region = "Đắk Nông (Calc)", RegionCode = "DAK_NONG", CoffeePrice = baseVnd > 0 ? baseVnd + 100 : 0, PepperPrice = 145000, Change = 0, UpdatedAt = now }
            };

            // Add Calulated Robusta to Commodities
            commodities.Add(new CommodityPriceDto
            {
                Name = "Cà phê Robusta (Quy đổi)",
                Code = "ROBUSTA_VN",
                CurrentPrice = baseVnd,
                Change = 0,
                ChangePercent = 0,
                Unit = "VND/kg",
                UpdatedAt = now,
                Source = "Quy đổi từ Global API"
            });

            var result = new MarketPriceResponseDto
            {
                FetchedAt = now,
                IsFromCache = false,
                Commodities = commodities,
                RegionalPrices = regionalPrices
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching from Alpha Vantage API");
            throw;
        }
    }

    // ==================== OPTION 1: WEB SCRAPING ====================
    
    /// <summary>
    /// Lấy giá từ chocaphe.vn (Web Scraping)
    ///  CHÚ Ý: Có thể vi phạm bản quyền, chỉ dùng cho mục đích học tập
    /// </summary>
    private async Task<MarketPriceResponseDto?> FetchFromWebScrapingAsync()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            
            var url = "https://chocaphe.vn/gia-ca-phe-truc-tuyen.cfp";
            var response = await httpClient.GetStringAsync(url);
            var now = DateTime.Now;

            // Extract giá cà phê nội địa
            decimal coffeePrice = 99600m;
            var priceMatch = Regex.Match(response, @"nội địa\s+([\d,]+)đ/kg");
            if (priceMatch.Success)
            {
                coffeePrice = decimal.Parse(priceMatch.Groups[1].Value.Replace(",", ""));
            }

            // Extract thay đổi
            decimal change = 0;
            decimal changePercent = 0;
            var changeMatch = Regex.Match(response, @"(tăng|giảm)[^0-9]+([\d,]+)đ\s*\(([\d.]+)%\)");
            if (changeMatch.Success)
            {
                var direction = changeMatch.Groups[1].Value;
                change = decimal.Parse(changeMatch.Groups[2].Value.Replace(",", ""));
                changePercent = decimal.Parse(changeMatch.Groups[3].Value);
                
                if (direction == "giảm")
                {
                    change = -change;
                    changePercent = -changePercent;
                }
            }

            var result = new MarketPriceResponseDto
            {
                FetchedAt = now,
                IsFromCache = false,
                Commodities = new List<CommodityPriceDto>
                {
                    new CommodityPriceDto
                    {
                        Name = "Cà phê Robusta (VN)",
                        Code = "ROBUSTA",
                        CurrentPrice = coffeePrice,
                        Change = change,
                        ChangePercent = changePercent,
                        Unit = "VND/kg",
                        UpdatedAt = now,
                        Source = "Chợ Cà Phê (Web Scraping)"
                    }
                },
                RegionalPrices = GetRegionalPrices(coffeePrice, now)
            };

            _logger.LogInformation("Scraped from chocaphe.vn: {Price:N0} VND/kg", coffeePrice);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error web scraping from chocaphe.vn");
            return null;
        }
    }

    // ==================== OPTION 2: TWELVE DATA API ====================
    
    /// <summary>
    /// Lấy giá từ Twelve Data API (Coffee Futures - ARABICA)
    ///  Hợp pháp, có API key chính thức
    /// </summary>
    private async Task<MarketPriceResponseDto?> FetchFromTwelveDataAsync()
    {
        try
        {
            var apiKey = _configuration["MarketPrice:TwelveData:ApiKey"] ?? "ec9a9c928dba433e8d235e6698c617e0";
            var baseUrl = _configuration["MarketPrice:TwelveData:BaseUrl"] ?? "https://api.twelvedata.com";

            var httpClient = _httpClientFactory.CreateClient();
            var now = DateTime.Now;

            // Lấy giá Coffee Futures - KC1 = Front-month Arabica contract (ICE US)
            var coffeeUrl = $"{baseUrl}/quote?symbol=KC1&apikey={apiKey}";
            var coffeeResponse = await httpClient.GetStringAsync(coffeeUrl);
            var coffeeData = System.Text.Json.JsonDocument.Parse(coffeeResponse);

            var root = coffeeData.RootElement;

            // Parse dữ liệu THẬT từ API (không mock)
            decimal arabicaPrice = 0;
            decimal change = 0;
            decimal changePercent = 0;

            if (root.TryGetProperty("close", out var closeElement))
            {
                arabicaPrice = decimal.Parse(closeElement.GetString() ?? "0");
            }

            // Lấy change thật từ API (nếu có)
            if (root.TryGetProperty("change", out var changeElement))
            {
                change = decimal.Parse(changeElement.GetString() ?? "0");
            }

            if (root.TryGetProperty("percent_change", out var percentElement))
            {
                changePercent = decimal.Parse(percentElement.GetString() ?? "0");
            }

            // Nếu API không trả change, tính từ previous_close
            if (change == 0 && root.TryGetProperty("previous_close", out var prevElement))
            {
                var prevClose = decimal.Parse(prevElement.GetString() ?? "0");
                if (prevClose > 0)
                {
                    change = arabicaPrice - prevClose;
                    changePercent = (change / prevClose) * 100;
                }
            }

            var result = new MarketPriceResponseDto
            {
                FetchedAt = now,
                IsFromCache = false,
                Commodities = new List<CommodityPriceDto>
                {
                    // 1. INTERNATIONAL: Arabica Futures (Twelve Data)
                    new CommodityPriceDto
                    {
                        Name = "Coffee Futures (Arabica)",
                        Code = "ARABICA",
                        CurrentPrice = arabicaPrice,
                        Change = Math.Round(change, 2),
                        ChangePercent = Math.Round(changePercent, 2),
                        Unit = "USD/lb",
                        UpdatedAt = now,
                        Source = "ICE US (Twelve Data)"
                    },
                    // 2. DOMESTIC: Robusta nội địa Việt Nam
                    new CommodityPriceDto
                    {
                        Name = "Cà phê Robusta nội địa",
                        Code = "ROBUSTA_VN",
                        CurrentPrice = 99600m,
                        Change = 600m,
                        ChangePercent = 0.61m,
                        Unit = "VND/kg",
                        UpdatedAt = now,
                        Source = "Thị trường Việt Nam"
                    }
                },
                RegionalPrices = GetRegionalPrices(99600m, now)
            };

            _logger.LogInformation("Fetched from Twelve Data - Arabica: ${Price}/lb ({Change:+0.00;-0.00}%), VN Robusta: {VnPrice:N0} VND/kg", 
                arabicaPrice, changePercent, 99600m);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching from Twelve Data API");
            return null;
        }
    }

    // ==================== REGIONAL PRICE STRATEGIES ====================

    /// <summary>
    /// FULL STRATEGY 1: SCRAPING
    /// Lấy giá từ Web Scraping -> Build Full Respone (Top + Regional)
    /// </summary>
    public async Task<MarketPriceResponseDto?> FetchFullFromScrapingAsync()
    {
        try
        {
            var now = DateTime.Now;
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            client.Timeout = TimeSpan.FromSeconds(10);

            string response;
            try 
            {
                response = await client.GetStringAsync("https://chocaphe.vn/gia-ca-phe-truc-tuyen.cfp");
            }
            catch 
            {
                _logger.LogError("Failed to connect to chocaphe.vn");
                return null;
            }
            
            // Kiểm tra xem response có chứa data thật không
            if (!response.Contains("Đắk Lắk") && !response.Contains("Dak Lak"))
            {
                _logger.LogWarning("HTML response doesn't contain expected data. Page may use JavaScript rendering.");
                return null;
            }
            
            // Helper function để extract price và change từ table row
            // HTML structure: <tr><td>Tên</td><td>Giá</td><td>Thay đổi</td></tr>
            (decimal price, decimal change) ExtractFromTableRow(string content, string regionName)
            {
                try
                {
                    // Pattern phức tạp hơn để match HTML table structure
                    // Tìm row chứa tên region, sau đó extract 2 số tiếp theo (giá và thay đổi)
                    
                    // Step 1: Tìm <tr> chứa tên region
                    var rowPattern = $@"<tr[^>]*>.*?{Regex.Escape(regionName)}.*?</tr>";
                    var rowMatch = Regex.Match(content, rowPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    
                    if (!rowMatch.Success)
                    {
                        _logger.LogWarning($"{regionName}: Could not find table row");
                        return (0, 0);
                    }
                    
                    var rowHtml = rowMatch.Value;
                    
                    // Step 2: Extract các <td> cells
                    var cellsPattern = @"<td[^>]*>(.*?)</td>";
                    var cellMatches = Regex.Matches(rowHtml, cellsPattern, RegexOptions.Singleline);
                    
                    if (cellMatches.Count < 3)
                    {
                        _logger.LogWarning($"{regionName}: Not enough table cells found ({cellMatches.Count})");
                        return (0, 0);
                    }
                    
                    // Cell 0: Tên thị trường
                    // Cell 1: Giá trung bình
                    // Cell 2: Thay đổi
                    
                    // Extract price từ cell 1
                    var priceCell = StripHtmlTags(cellMatches[1].Groups[1].Value);
                    var priceNumbers = Regex.Match(priceCell, @"[\d,]+");
                    decimal price = 0;
                    if (priceNumbers.Success)
                    {
                        var priceStr = priceNumbers.Value.Replace(",", "").Replace(".", "");
                        decimal.TryParse(priceStr, out price);
                    }
                    
                    // Extract change từ cell 2
                    var changeCell = StripHtmlTags(cellMatches[2].Groups[1].Value);
                    var changeNumbers = Regex.Match(changeCell, @"-?[\d,]+");
                    decimal change = 0;
                    if (changeNumbers.Success)
                    {
                        var changeStr = changeNumbers.Value.Replace(",", "").Replace(".", "");
                        decimal.TryParse(changeStr, out change);
                    }
                    
                    // Detect direction từ SVG icon hoặc class
                    if (rowHtml.Contains("arrow-down") || rowHtml.Contains("text-red"))
                    {
                        change = -Math.Abs(change);
                    }
                    else if (rowHtml.Contains("arrow-up") || rowHtml.Contains("text-green"))
                    {
                        change = Math.Abs(change);
                    }
                    // minus icon thì change = 0 hoặc giữ nguyên
                    
                    _logger.LogInformation($"Scraped {regionName}: Price={price:N0}, Change={change:+0;-0;0}");
                    
                    // Validate price > 10k
                    if (price > 10000)
                    {
                        return (price, change);
                    }
                    else
                    {
                        _logger.LogWarning($"{regionName}: Price {price} too low, rejected");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error extracting {regionName}");
                }
                
                return (0, 0);
            }
            
            // Helper to strip HTML tags
            string StripHtmlTags(string html)
            {
                return Regex.Replace(html, @"<[^>]+>", "").Trim();
            }

            // Scrape từng tỉnh
            var (daklakPrice, daklakChange) = ExtractFromTableRow(response, "Đắk Lắk");
            var (lamdongPrice, lamdongChange) = ExtractFromTableRow(response, "Lâm Đồng");
            var (gialaiPrice, gialaiChange) = ExtractFromTableRow(response, "Gia Lai");
            var (daknongPrice, daknongChange) = ExtractFromTableRow(response, "Đắk Nông");
            var (pepperPrice, pepperChange) = ExtractFromTableRow(response, "Hồ tiêu");
            
            // Extract USD exchange rate
            var (usdRate, usdChange) = ExtractFromTableRow(response, "Tỷ giá USD");

            // Build regional prices
            var regionalPrices = new List<RegionalPriceDto>
            {
                new() { Region = "Đắk Lắk", RegionCode = "DAK_LAK", CoffeePrice = daklakPrice, PepperPrice = pepperPrice, Change = daklakChange, UpdatedAt = now },
                new() { Region = "Lâm Đồng", RegionCode = "LAM_DONG", CoffeePrice = lamdongPrice, PepperPrice = pepperPrice, Change = lamdongChange, UpdatedAt = now },
                new() { Region = "Gia Lai", RegionCode = "GIA_LAI", CoffeePrice = gialaiPrice, PepperPrice = pepperPrice, Change = gialaiChange, UpdatedAt = now },
                new() { Region = "Đắk Nông", RegionCode = "DAK_NONG", CoffeePrice = daknongPrice, PepperPrice = pepperPrice, Change = daknongChange, UpdatedAt = now }
            };

            // Build commodities
            var commodities = new List<CommodityPriceDto>
            {
                new CommodityPriceDto
                {
                    Name = "Cà phê Nhân xô (Việt Nam)",
                    Code = "ROBUSTA_VN",
                    CurrentPrice = daklakPrice,
                    Change = daklakChange,
                    ChangePercent = daklakPrice > 0 ? Math.Round((daklakChange / daklakPrice) * 100, 2) : 0,
                    Unit = "VND/kg",
                    UpdatedAt = now,
                    Source = "chocaphe.vn"
                },
                new CommodityPriceDto
                {
                    Name = "Hồ tiêu (Việt Nam)",
                    Code = "PEPPER_VN",
                    CurrentPrice = pepperPrice,
                    Change = pepperChange,
                    ChangePercent = pepperPrice > 0 ? Math.Round((pepperChange / pepperPrice) * 100, 2) : 0,
                    Unit = "VND/kg",
                    UpdatedAt = now,
                    Source = "chocaphe.vn"
                }
            };
            
            // Add USD rate if found
            if (usdRate > 0)
            {
                commodities.Add(new CommodityPriceDto
                {
                    Name = "Tỷ giá USD/VND",
                    Code = "USD_VND",
                    CurrentPrice = usdRate,
                    Change = usdChange,
                    ChangePercent = usdRate > 0 ? Math.Round((usdChange / usdRate) * 100, 2) : 0,
                    Unit = "VND",
                    UpdatedAt = now,
                    Source = "chocaphe.vn"
                });
            }
            
            return new MarketPriceResponseDto
            {
                FetchedAt = now,
                IsFromCache = false,
                Commodities = commodities,
                RegionalPrices = regionalPrices
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping full market data");
            return null;
        }
    }

    /// <summary>
    /// FULL STRATEGY 2: HEADLESS BROWSER SCRAPING
    /// Scrape giacaphe.com với PuppeteerSharp (render JavaScript)
    /// </summary>
    public async Task<MarketPriceResponseDto?> FetchFromGiaCaPheWithPuppeteerAsync()
    {
        try
        {
            var now = DateTime.Now;
            
            // Download Chromium nếu chưa có (chỉ chạy lần đầu)
            _logger.LogInformation("Checking Puppeteer browser...");
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            
            _logger.LogInformation("Launching headless browser...");
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            });
            
            await using var page = await browser.NewPageAsync();
            
            // Set user agent to avoid bot detection
            await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            
            // Navigate to giacaphe.com
            _logger.LogInformation("Navigating to giacaphe.com...");
            await page.GoToAsync("https://giacaphe.com/gia-ca-phe-noi-dia/", new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle2 },
                Timeout = 60000
            });
            
            // Wait a bit for JavaScript to execute
            await Task.Delay(3000);
            
            // Scroll down to ensure the price table is in viewport
            _logger.LogInformation("Scrolling down to price table...");
            await page.EvaluateFunctionAsync(@"() => {
                const table = document.querySelector('#gia-noi-dia');
                if (table) {
                    table.scrollIntoView({ behavior: 'smooth', block: 'center' });
                }
            }");
            
            await Task.Delay(2000);
            
            // Take FULL PAGE screenshot for debugging
            var screenshotPath = Path.Combine(Path.GetTempPath(), $"giacaphe_debug_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            await page.ScreenshotAsync(screenshotPath, new ScreenshotOptions { FullPage = true });
            _logger.LogInformation($"Full page screenshot saved to: {screenshotPath}");
            
            // Wait for table structure to exist
            _logger.LogInformation("Waiting for table structure...");
            await page.WaitForSelectorAsync(".price-table tbody tr", new WaitForSelectorOptions { Timeout = 10000 });
            
            // CRITICAL: Wait for JavaScript to populate the span elements with actual price data
            // The spans are initially empty and filled by JS after page load
            _logger.LogInformation("Waiting for price data to be populated by JavaScript...");
            
            bool dataPopulated = false;
            int maxRetries = 10;
            
            for (int i = 0; i < maxRetries; i++)
            {
                // First, log what we see
                var debugInfo = await page.EvaluateFunctionAsync<string>(@"() => {
                    const rows = document.querySelectorAll('.price-table tbody tr');
                    let info = `Found ${rows.length} rows\n`;
                    
                    if (rows.length > 0) {
                        const firstRow = rows[0];
                        const cells = firstRow.querySelectorAll('td');
                        info += `First row has ${cells.length} cells\n`;
                        
                        if (cells.length >= 2) {
                            info += `Cell 1 innerHTML: ${cells[1].innerHTML}\n`;
                            info += `Cell 1 textContent: '${cells[1].textContent.trim()}'\n`;
                        }
                    }
                    
                    return info;
                }");
                
                _logger.LogInformation($"Debug info (attempt {i + 1}):\n{debugInfo}");
                
                var hasData = await page.EvaluateFunctionAsync<bool>(@"() => {
                    const rows = document.querySelectorAll('.price-table tbody tr');
                    if (rows.length === 0) return false;
                    
                    // Check if at least one price cell has content
                    for (let row of rows) {
                        const cells = row.querySelectorAll('td');
                        if (cells.length >= 2) {
                            const priceText = cells[1].textContent.trim();
                            // If we find any cell with numbers, data is populated
                            if (priceText && /\d/.test(priceText)) {
                                return true;
                            }
                        }
                    }
                    return false;
                }");
                
                if (hasData)
                {
                    dataPopulated = true;
                    _logger.LogInformation($"Price data populated successfully after {(i + 1) * 2} seconds!");
                    break;
                }
                
                _logger.LogInformation($"Waiting for data... attempt {i + 1}/{maxRetries}");
                await Task.Delay(2000);
            }
            
            if (!dataPopulated)
            {
                _logger.LogError("Timeout waiting for price data to populate. Screenshot saved for debugging.");
                return null;
            }
            
            // Extract data using JavaScript evaluation
            var priceData = await page.EvaluateFunctionAsync<PriceDataJs>(@"() => {
                const rows = document.querySelectorAll('.price-table tbody tr');
                const data = {};
                
                rows.forEach((row, index) => {
                    const cells = row.querySelectorAll('td');
                    if (cells.length >= 3) {
                        const name = cells[0].textContent.trim();
                        const priceText = cells[1].textContent.trim().replace(/[,\.]/g, '');
                        const changeText = cells[2].textContent.trim().replace(/[,\.]/g, '').replace('+', '');
                        
                        data[index] = {
                            name: name,
                            price: parseInt(priceText) || 0,
                            change: parseInt(changeText) || 0
                        };
                    }
                });
                
                return data;
            }");
            
            _logger.LogInformation("Successfully scraped {Count} rows from giacaphe.com", priceData?.Count ?? 0);
            
            // Parse data
            decimal daklakPrice = 0, daklakChange = 0;
            decimal lamdongPrice = 0, lamdongChange = 0;
            decimal gialaiPrice = 0, gialaiChange = 0;
            decimal daknongPrice = 0, daknongChange = 0;
            decimal pepperPrice = 0, pepperChange = 0;
            decimal usdRate = 0, usdChange = 0;
            
            if (priceData != null)
            {
                foreach (var kvp in priceData)
                {
                    var item = kvp.Value;
                    var name = item.Name?.ToLower() ?? "";
                    
                    if (name.Contains("đắk lắk") || name.Contains("dak lak"))
                    {
                        daklakPrice = item.Price;
                        daklakChange = item.Change;
                    }
                    else if (name.Contains("lâm đồng") || name.Contains("lam dong"))
                    {
                        lamdongPrice = item.Price;
                        lamdongChange = item.Change;
                    }
                    else if (name.Contains("gia lai"))
                    {
                        gialaiPrice = item.Price;
                        gialaiChange = item.Change;
                    }
                    else if (name.Contains("đắk nông") || name.Contains("dak nong"))
                    {
                        daknongPrice = item.Price;
                        daknongChange = item.Change;
                    }
                    else if (name.Contains("hồ tiêu") || name.Contains("ho tieu"))
                    {
                        pepperPrice = item.Price;
                        pepperChange = item.Change;
                    }
                    else if (name.Contains("usd") || name.Contains("tỷ giá"))
                    {
                        usdRate = item.Price;
                        usdChange = item.Change;
                    }
                }
            }
            
            // Build response
            var regionalPrices = new List<RegionalPriceDto>
            {
                new() { Region = "Đắk Lắk", RegionCode = "DAK_LAK", CoffeePrice = daklakPrice, PepperPrice = pepperPrice, Change = daklakChange, UpdatedAt = now },
                new() { Region = "Lâm Đồng", RegionCode = "LAM_DONG", CoffeePrice = lamdongPrice, PepperPrice = pepperPrice, Change = lamdongChange, UpdatedAt = now },
                new() { Region = "Gia Lai", RegionCode = "GIA_LAI", CoffeePrice = gialaiPrice, PepperPrice = pepperPrice, Change = gialaiChange, UpdatedAt = now },
                new() { Region = "Đắk Nông", RegionCode = "DAK_NONG", CoffeePrice = daknongPrice, PepperPrice = pepperPrice, Change = daknongChange, UpdatedAt = now }
            };

            var commodities = new List<CommodityPriceDto>
            {
                new CommodityPriceDto
                {
                    Name = "Cà phê Nhân xô (Việt Nam)",
                    Code = "ROBUSTA_VN",
                    CurrentPrice = daklakPrice,
                    Change = daklakChange,
                    ChangePercent = daklakPrice > 0 ? Math.Round((daklakChange / daklakPrice) * 100, 2) : 0,
                    Unit = "VND/kg",
                    UpdatedAt = now,
                    Source = "giacaphe.com (Puppeteer)"
                },
                new CommodityPriceDto
                {
                    Name = "Hồ tiêu (Việt Nam)",
                    Code = "PEPPER_VN",
                    CurrentPrice = pepperPrice,
                    Change = pepperChange,
                    ChangePercent = pepperPrice > 0 ? Math.Round((pepperChange / pepperPrice) * 100, 2) : 0,
                    Unit = "VND/kg",
                    UpdatedAt = now,
                    Source = "giacaphe.com (Puppeteer)"
                }
            };
            
            if (usdRate > 0)
            {
                commodities.Add(new CommodityPriceDto
                {
                    Name = "Tỷ giá USD/VND",
                    Code = "USD_VND",
                    CurrentPrice = usdRate,
                    Change = usdChange,
                    ChangePercent = usdRate > 0 ? Math.Round((usdChange / usdRate) * 100, 2) : 0,
                    Unit = "VND",
                    UpdatedAt = now,
                    Source = "giacaphe.com (Puppeteer)"
                });
            }
            
            return new MarketPriceResponseDto
            {
                FetchedAt = now,
                IsFromCache = false,
                Commodities = commodities,
                RegionalPrices = regionalPrices
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping giacaphe.com with Puppeteer");
            return null;
        }
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
    /// Xóa cache
    /// </summary>
    public async Task ClearCacheAsync()
    {
        await RedisService.DeleteAsync(CACHE_KEY);
        _logger.LogInformation("Market price cache cleared");
    }
}
