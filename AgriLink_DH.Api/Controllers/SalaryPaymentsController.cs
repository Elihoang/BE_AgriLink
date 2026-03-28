using AgriLink_DH.Api.Extensions;
using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.SalaryPayment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/salary-payments")]
[Authorize]
public class SalaryPaymentsController : ControllerBase
{
    private readonly SalaryPaymentService _salaryPaymentService;
    private readonly ILogger<SalaryPaymentsController> _logger;

    public SalaryPaymentsController(SalaryPaymentService salaryPaymentService, ILogger<SalaryPaymentsController> logger)
    {
        _salaryPaymentService = salaryPaymentService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<SalaryPaymentDto>>>> GetPayments()
    {
        try
        {
            var payments = await _salaryPaymentService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<SalaryPaymentDto>>.SuccessResponse(payments, "Lấy lịch sử thanh toán lương thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy lịch sử thanh toán lương");
            return StatusCode(500, ApiResponse<IEnumerable<SalaryPaymentDto>>.ErrorResponse("Lỗi khi lấy lịch sử thanh toán lương", 500));
        }
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<ApiResponse<SalaryCalculationResultDto>>> CalculateSalary([FromBody] CalculateSalaryRequestDto request)
    {
        try
        {
            var result = await _salaryPaymentService.CalculateSalaryAsync(request);
            return Ok(ApiResponse<SalaryCalculationResultDto>.SuccessResponse(result, "Tính toán lương thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SalaryCalculationResultDto>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tính toán lương cho worker {WorkerId}", request.WorkerId);
            return StatusCode(500, ApiResponse<SalaryCalculationResultDto>.ErrorResponse("Lỗi khi tính toán lương", 500));
        }
    }

    [HttpPost("execute")]
    public async Task<ActionResult<ApiResponse<SalaryPaymentDto>>> ExecutePayment([FromBody] ExecutePaymentRequestDto request)
    {
        try
        {
            var result = await _salaryPaymentService.ExecutePaymentAsync(request);
            return Ok(ApiResponse<SalaryPaymentDto>.SuccessResponse(result, "Thanh toán lương qua MoMo thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SalaryPaymentDto>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thực hiện thanh toán lương cho worker {WorkerId}", request.WorkerId);
            return StatusCode(500, ApiResponse<SalaryPaymentDto>.ErrorResponse("Lỗi khi thực hiện thanh toán lương", 500));
        }
    }
}
