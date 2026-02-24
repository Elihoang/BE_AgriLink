using AgriLink_DH.Core.Helpers;
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

    /// <summary>
    /// [ADMIN] Lấy giá hôm trước để tính toán Change tự động
    /// </summary>
    [HttpGet("previous-prices")]
    public async Task<ActionResult<ApiResponse<object>>> GetPreviousDayPrices()
    {
        try
        {
            var previousPrices = await _marketPriceService.GetPreviousDayPricesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(
                previousPrices,
                "Lấy giá hôm trước thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting previous day prices");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "Lỗi khi lấy giá hôm trước", 500));
        }
    }

    // ═══════════════════════════════════════════════════════
    // EXCEL IMPORT / TEMPLATE DOWNLOAD
    // ═══════════════════════════════════════════════════════

    /// <summary>Tải Excel mẫu cà phê – GET /api/MarketPrice/admin/template/coffee</summary>
    [HttpGet("admin/template/coffee")]
    public IActionResult DownloadCoffeeTemplate()
    {
        var bytes = ExcelPriceHelper.GenerateCoffeeTemplate();
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"template-ca-phe-{DateTime.Today:yyyy-MM-dd}.xlsx");
    }

    /// <summary>Tải Excel mẫu hồ tiêu – GET /api/MarketPrice/admin/template/pepper</summary>
    [HttpGet("admin/template/pepper")]
    public IActionResult DownloadPepperTemplate()
    {
        var bytes = ExcelPriceHelper.GeneratePepperTemplate();
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"template-ho-tieu-{DateTime.Today:yyyy-MM-dd}.xlsx");
    }

    /// <summary>Import cà phê từ Excel – POST /api/MarketPrice/admin/import/coffee</summary>
    [HttpPost("admin/import/coffee")]
    public async Task<ActionResult<ApiResponse<object>>> ImportCoffeeExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.ErrorResponse("Vui lòng chọn file Excel (.xlsx).", 400));
        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(ApiResponse<object>.ErrorResponse("Chỉ chấp nhận file .xlsx.", 400));
        try
        {
            // Lookup productId từ DB theo code (không hardcode GUID)
            Guid coffeeId;
            try { coffeeId = await _marketPriceService.GetProductIdByCodeAsync("CF_ROBUSTA"); }
            catch (KeyNotFoundException ex) { return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, 400)); }

            using var stream = file.OpenReadStream();
            var pr           = ExcelPriceHelper.ParseCoffeeExcel(stream, coffeeId);
            if (pr.ValidRows.Count == 0 && pr.HasErrors)
                return BadRequest(ApiResponse<object>.ErrorResponse($"File không hợp lệ: {pr.Errors.Count} lỗi.", 400));

            var updatedBy = User.Identity?.Name ?? "Admin";
            var saved = 0; var saveErrs = new List<string>();
            foreach (var req in pr.ValidRows)
            {
                try   { await _marketPriceService.UpdatePriceAsync(req, updatedBy); saved++; }
                catch (Exception ex) { saveErrs.Add($"{req.RegionCode}: {ex.Message}"); }
            }
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                imported = saved, parseErrors = pr.Errors, saveErrors = saveErrs,
                totalRows = pr.ValidRows.Count + pr.Errors.Count,
            }, $"Import Cà phê: {saved} dòng thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi import Excel cà phê");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Lỗi xử lý file.", 500));
        }
    }

    /// <summary>Import hồ tiêu từ Excel – POST /api/MarketPrice/admin/import/pepper</summary>
    [HttpPost("admin/import/pepper")]
    public async Task<ActionResult<ApiResponse<object>>> ImportPepperExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.ErrorResponse("Vui lòng chọn file Excel (.xlsx).", 400));
        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(ApiResponse<object>.ErrorResponse("Chỉ chấp nhận file .xlsx.", 400));
        try
        {
            // Lookup productId từ DB theo code (không hardcode GUID)
            Guid pepperId;
            try { pepperId = await _marketPriceService.GetProductIdByCodeAsync("PEPPER"); }
            catch (KeyNotFoundException ex) { return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, 400)); }

            using var stream = file.OpenReadStream();
            var pr           = ExcelPriceHelper.ParsePepperExcel(stream, pepperId);
            if (pr.ValidRows.Count == 0 && pr.HasErrors)
                return BadRequest(ApiResponse<object>.ErrorResponse($"File không hợp lệ: {pr.Errors.Count} lỗi.", 400));

            var updatedBy = User.Identity?.Name ?? "Admin";
            var saved = 0; var saveErrs = new List<string>();
            foreach (var req in pr.ValidRows)
            {
                try   { await _marketPriceService.UpdatePriceAsync(req, updatedBy); saved++; }
                catch (Exception ex) { saveErrs.Add($"{req.RecordedDate:yyyy-MM-dd}: {ex.Message}"); }
            }
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                imported = saved, parseErrors = pr.Errors, saveErrors = saveErrs,
                totalRows = pr.ValidRows.Count + pr.Errors.Count,
            }, $"Import Hồ Tiêu: {saved} dòng thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi import Excel hồ tiêu");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Lỗi xử lý file.", 500));
        }
    }
}
