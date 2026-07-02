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
        var sales = await _farmSaleService.GetBySeasonAsync(seasonId);
        return Ok(ApiResponse<IEnumerable<FarmSaleDto>>.SuccessResponse(sales, "Lấy danh sách bán hàng thành công"));
    }

    [HttpGet("total-revenue/{seasonId:guid}")]
    public async Task<ActionResult<ApiResponse<decimal>>> GetTotalRevenue(Guid seasonId)
    {
        var total = await _farmSaleService.GetTotalRevenueAsync(seasonId);
        return Ok(ApiResponse<decimal>.SuccessResponse(total, "Tính tổng doanh thu thành công"));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<FarmSaleDto>>> GetById(Guid id)
    {
        var sale = await _farmSaleService.GetByIdAsync(id);
        if (sale == null)
        {
            return NotFound(ApiResponse<FarmSaleDto>.NotFoundResponse($"Không tìm thấy phiếu bán hàng với ID: {id}"));
        }

        return Ok(ApiResponse<FarmSaleDto>.SuccessResponse(sale, "Lấy thông tin bán hàng thành công"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<FarmSaleDto>>> Create([FromBody] CreateFarmSaleDto dto)
    {
        var sale = await _farmSaleService.CreateSaleAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = sale.Id },
            ApiResponse<FarmSaleDto>.CreatedResponse(sale, "Tạo phiếu bán hàng mới thành công"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _farmSaleService.DeleteSaleAsync(id);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Xóa phiếu bán hàng thành công"));
    }
    
    [HttpPut("{id:guid}/soft-delete")]
    public async Task<ActionResult<ApiResponse<bool>>> SoftDeleteSale(Guid id)
    {
        var result = await _farmSaleService.SoftDeleteSaleAsync(id);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Xóa mềm phiếu bán hàng thành công"));
    }

    [HttpPut("{id:guid}/restore")]
    public async Task<ActionResult<ApiResponse<bool>>> RestoreSale(Guid id)
    {
        var result = await _farmSaleService.RestoreSaleAsync(id);
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Khôi phục phiếu bán hàng thành công"));
    }
}
