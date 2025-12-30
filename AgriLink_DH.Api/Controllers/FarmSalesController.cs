using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.FarmSale;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FarmSalesController : ControllerBase
{
    private readonly FarmSaleService _farmSaleService;
    private readonly ILogger<FarmSalesController> _logger;

    public FarmSalesController(
        FarmSaleService farmSaleService,
        ILogger<FarmSalesController> logger)
    {
        _farmSaleService = farmSaleService;
        _logger = logger;
    }

    [HttpGet("by-season/{seasonId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<FarmSaleDto>>>> GetBySeason(Guid seasonId)
    {
        try
        {
            var sales = await _farmSaleService.GetBySeasonAsync(seasonId);
            return Ok(ApiResponse<IEnumerable<FarmSaleDto>>.SuccessResponse(sales, "Lấy danh sách bán hàng thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách bán hàng");
            return StatusCode(500, ApiResponse<IEnumerable<FarmSaleDto>>.ErrorResponse("Lỗi khi lấy danh sách bán hàng", 500));
        }
    }

    [HttpGet("total-revenue/{seasonId:guid}")]
    public async Task<ActionResult<ApiResponse<decimal>>> GetTotalRevenue(Guid seasonId)
    {
        try
        {
            var total = await _farmSaleService.GetTotalRevenueAsync(seasonId);
            return Ok(ApiResponse<decimal>.SuccessResponse(total, "Tính tổng doanh thu thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tính tổng doanh thu");
            return StatusCode(500, ApiResponse<decimal>.ErrorResponse("Lỗi khi tính tổng doanh thu", 500));
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<FarmSaleDto>>> GetById(Guid id)
    {
        try
        {
            var sale = await _farmSaleService.GetByIdAsync(id);
            if (sale == null)
            {
                return NotFound(ApiResponse<FarmSaleDto>.NotFoundResponse($"Không tìm thấy phiếu bán hàng với ID: {id}"));
            }

            return Ok(ApiResponse<FarmSaleDto>.SuccessResponse(sale, "Lấy thông tin bán hàng thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin bán hàng");
            return StatusCode(500, ApiResponse<FarmSaleDto>.ErrorResponse("Lỗi khi lấy thông tin bán hàng", 500));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<FarmSaleDto>>> Create([FromBody] CreateFarmSaleDto dto)
    {
        try
        {
            var sale = await _farmSaleService.CreateSaleAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = sale.Id },
                ApiResponse<FarmSaleDto>.CreatedResponse(sale, "Tạo phiếu bán hàng mới thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<FarmSaleDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo phiếu bán hàng mới");
            return StatusCode(500, ApiResponse<FarmSaleDto>.ErrorResponse("Lỗi khi tạo phiếu bán hàng mới", 500));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var result = await _farmSaleService.DeleteSaleAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Xóa phiếu bán hàng thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa phiếu bán hàng");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi xóa phiếu bán hàng", 500));
        }
    }
    
    [HttpPut("{id:guid}/soft-delete")]
    public async Task<ActionResult<ApiResponse<bool>>> SoftDeleteSale(Guid id)
    {
        try
        {
            var result = await _farmSaleService.SoftDeleteSaleAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Xóa mềm phiếu bán hàng thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa mềm phiếu bán hàng {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi xóa mềm phiếu bán hàng", 500));
        }
    }

    [HttpPut("{id:guid}/restore")]
    public async Task<ActionResult<ApiResponse<bool>>> RestoreSale(Guid id)
    {
        try
        {
            var result = await _farmSaleService.RestoreSaleAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Khôi phục phiếu bán hàng thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi khôi phục phiếu bán hàng {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi khôi phục phiếu bán hàng", 500));
        }
    }
}
