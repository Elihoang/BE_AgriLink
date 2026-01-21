using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.MarketPrice;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarketPriceController : ControllerBase
{
    private readonly MarketPriceService _marketPriceService;
    private readonly ILogger<MarketPriceController> _logger;

    public MarketPriceController(
        MarketPriceService marketPriceService,
        ILogger<MarketPriceController> logger)
    {
        _marketPriceService = marketPriceService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy giá thị trường (tự động chọn provider từ config)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<MarketPriceResponseDto>>> GetMarketPrices()
    {
        try
        {
            var prices = await _marketPriceService.GetMarketPricesAsync();

            if (prices == null)
            {
                return NotFound(ApiResponse<MarketPriceResponseDto>.ErrorResponse(
                    "Không thể lấy dữ liệu giá thị trường", 404));
            }

            return Ok(ApiResponse<MarketPriceResponseDto>.SuccessResponse(
                prices,
                "Lấy giá thị trường thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting market prices");
            return StatusCode(500, ApiResponse<MarketPriceResponseDto>.ErrorResponse(
                "Lỗi khi lấy giá thị trường", 500));
        }
    }


    /// <summary>
    /// Lấy giá từ API (Alpha Vantage - Global Coffee)
    /// ✅ Hợp pháp, Free tier
    /// </summary>
    [HttpGet("api-data")]
    public async Task<ActionResult<ApiResponse<MarketPriceResponseDto>>> GetPricesFromAPI()
    {
        try
        {
            // Tạm thời gọi trực tiếp Twelve Data hoặc Alpha Vantage tùy cấu hình
            // Nhưng ở đây ta muốn test Alpha Vantage
            var prices = await _marketPriceService.GetPricesFromAlphaVantageAsync();

            if (prices == null)
            {
                return NotFound(ApiResponse<MarketPriceResponseDto>.ErrorResponse(
                    "Không thể lấy dữ liệu từ API Global", 404));
            }

            return Ok(ApiResponse<MarketPriceResponseDto>.SuccessResponse(
                prices,
                "Lấy giá từ Alpha Vantage API thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching from API");
            return StatusCode(500, ApiResponse<MarketPriceResponseDto>.ErrorResponse(
                "Lỗi khi gọi API", 500));
        }
    }

    // ==================== REGIONAL PRICE ENDPOINTS ====================

    /// <summary>
    /// Lấy giá Full từ Web Scraping
    /// </summary>
    [HttpGet("regional/scraping")]
    public async Task<ActionResult<ApiResponse<MarketPriceResponseDto>>> GetFullScraping()
    {
        var result = await _marketPriceService.FetchFullFromScrapingAsync();
        if (result == null) 
             return NotFound(ApiResponse<MarketPriceResponseDto>.ErrorResponse("Scraping failed or connection error", 404));

        return Ok(ApiResponse<MarketPriceResponseDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Lấy giá Full từ API Conversion
    /// </summary>
    [HttpGet("regional/conversion")]
    public async Task<ActionResult<ApiResponse<MarketPriceResponseDto>>> GetFullConversion()
    {
        // Reuse AlphaVantage logic which now returns Full DTO with conversion
        var result = await _marketPriceService.FetchFromAlphaVantageAsync(); 
        if (result == null)
             return NotFound(ApiResponse<MarketPriceResponseDto>.ErrorResponse("API Global failed", 404));
             
        return Ok(ApiResponse<MarketPriceResponseDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Lấy giá Full từ Manual Mock
    /// </summary>
    [HttpGet("regional/manual")]
    public async Task<ActionResult<ApiResponse<MarketPriceResponseDto>>> GetFullManual()
    {
        var result = await _marketPriceService.GetFullManualDataAsync();
        return Ok(ApiResponse<MarketPriceResponseDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Xóa cache và làm mới
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<MarketPriceResponseDto>>> RefreshMarketPrices()
    {
        try
        {
            await _marketPriceService.ClearCacheAsync();
            var prices = await _marketPriceService.GetMarketPricesAsync();

            if (prices == null)
            {
                return NotFound(ApiResponse<MarketPriceResponseDto>.ErrorResponse(
                    "Không thể lấy dữ liệu giá thị trường", 404));
            }

            return Ok(ApiResponse<MarketPriceResponseDto>.SuccessResponse(
                prices,
                "Làm mới giá thị trường thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing market prices");
            return StatusCode(500, ApiResponse<MarketPriceResponseDto>.ErrorResponse(
                "Lỗi khi làm mới giá thị trường", 500));
        }
    }
}
