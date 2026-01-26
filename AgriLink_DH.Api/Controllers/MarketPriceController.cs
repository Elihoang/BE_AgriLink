using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.MarketPrice;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarketPriceController : ControllerBase
{
    private readonly MarketPriceDbService _marketPriceService;
    private readonly ILogger<MarketPriceController> _logger;

    public MarketPriceController(
        MarketPriceDbService marketPriceService,
        ILogger<MarketPriceController> logger)
    {
        _marketPriceService = marketPriceService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy giá thị trường mới nhất từ database
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<MarketPriceResponseDto>>> GetMarketPrices()
    {
        try
        {
            var prices = await _marketPriceService.GetLatestPricesAsync();

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
    /// [ADMIN] Cập nhật giá thủ công
    /// </summary>
    [HttpPost("admin/update")]
    public async Task<ActionResult<ApiResponse<object>>> UpdatePrice([FromBody] UpdateMarketPriceRequest request)
    {
        try
        {
            // TODO: Add authentication/authorization for admin
            var updatedBy = User.Identity?.Name ?? "Admin";
            
            var result = await _marketPriceService.UpdatePriceAsync(request, updatedBy);

            return Ok(ApiResponse<object>.SuccessResponse(
                new 
                { 
                    id = result.Id, 
                    productId = result.ProductId,
                    region = result.Region,
                    price = result.Price,
                    recordedDate = result.RecordedDate
                },
                "Cập nhật giá thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating market price");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "Lỗi khi cập nhật giá", 500));
        }
    }

    /// <summary>
    /// [ADMIN] Cập nhật nhiều giá cùng lúc (Batch update)
    /// </summary>
    [HttpPost("admin/batch-update")]
    public async Task<ActionResult<ApiResponse<object>>> BatchUpdatePrices([FromBody] List<UpdateMarketPriceRequest> requests)
    {
        try
        {
            var updatedBy = User.Identity?.Name ?? "Admin";
            var results = new List<object>();

            foreach (var request in requests)
            {
                var result = await _marketPriceService.UpdatePriceAsync(request, updatedBy);
                results.Add(new 
                { 
                    id = result.Id, 
                    productId = result.ProductId,
                    region = result.Region,
                    price = result.Price
                });
            }

            return Ok(ApiResponse<object>.SuccessResponse(
                new { count = results.Count, items = results },
                $"Cập nhật thành công {results.Count} bản ghi"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error batch updating prices");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "Lỗi khi cập nhật giá hàng loạt", 500));
        }
    }

    /// <summary>
    /// Lấy lịch sử giá theo sản phẩm và khu vực
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<object>>> GetPriceHistory(
        [FromQuery] Guid productId,
        [FromQuery] string? regionCode = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int limit = 30)
    {
        try
        {
            var history = await _marketPriceService.GetPriceHistoryAsync(
                productId, regionCode, fromDate, toDate, limit);

            return Ok(ApiResponse<object>.SuccessResponse(
                history,
                $"Lấy lịch sử giá thành công ({history.Count} records)"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price history");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "Lỗi khi lấy lịch sử giá", 500));
        }
    }
}
