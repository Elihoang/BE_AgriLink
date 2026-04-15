using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgriLink_DH.Core.Configurations;
using AgriLink_DH.Domain.Common;
using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Share.DTOs.SalaryPayment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AgriLink_DH.Api.Controllers;

/// <summary>
/// Xử lý các callback từ MoMo Payment Gateway.
///
/// Gồm 2 endpoint:
///   - POST /api/momo/callback    : IPN — MoMo server gọi ngầm (cần public URL ở production)
///   - POST /api/momo/update-status : FE gọi sau khi MoMo redirect về (dùng khi localhost dev)
/// </summary>
[ApiController]
[Route("api/momo")]
public class MomoCallbackController : ControllerBase
{
    private readonly MomoSettings _settings;
    private readonly ISalaryPaymentRepository _salaryPaymentRepository;
    private readonly IWorkerAdvanceRepository _workerAdvanceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MomoCallbackController> _logger;

    public MomoCallbackController(
        IOptions<MomoSettings> settings,
        ISalaryPaymentRepository salaryPaymentRepository,
        IWorkerAdvanceRepository workerAdvanceRepository,
        IUnitOfWork unitOfWork,
        ILogger<MomoCallbackController> logger)
    {
        _settings = settings.Value;
        _salaryPaymentRepository = salaryPaymentRepository;
        _workerAdvanceRepository = workerAdvanceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // =====================================================================
    // IPN ENDPOINT — MoMo server gọi trực tiếp (production)
    // Xác thực bằng HMAC-SHA256 signature để đảm bảo request từ MoMo
    // =====================================================================
    [HttpPost("callback")]
    public async Task<IActionResult> HandleIpn([FromBody] JsonElement body)
    {
        _logger.LogInformation("[MOMO IPN] Received body: {Body}", body.ToString());

        try
        {
            var orderId      = GetString(body, "orderId");
            var transId      = GetString(body, "transId");
            var resultCode   = GetInt(body, "resultCode");
            var signature    = GetString(body, "signature");
            var partnerCode  = GetString(body, "partnerCode");
            var requestId    = GetString(body, "requestId");
            var amount       = GetLong(body, "amount");
            var orderInfo    = GetString(body, "orderInfo");
            var orderType    = GetString(body, "orderType");
            var message      = GetString(body, "message");
            var payType      = GetString(body, "payType");
            var responseTime = GetLong(body, "responseTime");
            var extraData    = GetString(body, "extraData");

            // Verify HMAC signature (alphabetical key order per MoMo docs)
            var rawSig =
                $"accessKey={_settings.AccessKey}" +
                $"&amount={amount}" +
                $"&extraData={extraData}" +
                $"&message={message}" +
                $"&orderId={orderId}" +
                $"&orderInfo={orderInfo}" +
                $"&orderType={orderType}" +
                $"&partnerCode={partnerCode}" +
                $"&payType={payType}" +
                $"&requestId={requestId}" +
                $"&responseTime={responseTime}" +
                $"&resultCode={resultCode}" +
                $"&transId={transId}";

            if (!Verify(rawSig, signature))
            {
                _logger.LogWarning("[MOMO IPN] Invalid signature");
                return BadRequest(new { message = "Invalid signature" });
            }

            _logger.LogInformation("[MOMO IPN] Valid. OrderId={OrderId} ResultCode={ResultCode}",
                orderId, resultCode);

            await UpdatePaymentStatusAsync(orderId, transId, resultCode);

            // MoMo yêu cầu luôn trả 200 để không retry
            return Ok(new { message = "Received" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MOMO IPN] Error");
            return Ok(new { message = "Error but acknowledged" }); // vẫn 200
        }
    }

    // =====================================================================
    // FE ENDPOINT — FE gọi sau redirect từ MoMo (thay IPN khi dev localhost)
    // Không verify signature để đơn giản; production nên dùng IPN phía trên
    // =====================================================================
    [HttpPost("update-status")]
    public async Task<IActionResult> UpdateStatus([FromBody] MomoReturnDto dto)
    {
        _logger.LogInformation("[MOMO RETURN] UpdateStatus: OrderId={OrderId} ResultCode={ResultCode}",
            dto.OrderId, dto.ResultCode);

        var status = await UpdatePaymentStatusAsync(dto.OrderId, dto.TransId, dto.ResultCode);

        return Ok(new { success = true, status });
    }

    // =====================================================================
    // PRIVATE HELPERS
    // =====================================================================

    /// <summary>Cập nhật status payment + deduct advances nếu Success.</summary>
    private async Task<string> UpdatePaymentStatusAsync(string orderId, string? transId, int resultCode)
    {
        var payments = await _salaryPaymentRepository.GetAllAsync();
        var payment  = payments.FirstOrDefault(p => p.MomoOrderId == orderId);

        if (payment == null)
        {
            _logger.LogWarning("[MOMO] Payment not found: {OrderId}", orderId);
            return "NotFound";
        }

        // Idempotent: chỉ update nếu đang chờ kết quả
        if (payment.Status != SalaryPaymentStatus.Processing &&
            payment.Status != SalaryPaymentStatus.Pending)
        {
            _logger.LogInformation("[MOMO] Skip update — already {Status}", payment.Status);
            return payment.Status.ToString();
        }

        payment.MomoTransId    = transId ?? payment.MomoTransId;
        payment.MomoResultCode = resultCode;
        payment.Status         = resultCode == 0
            ? SalaryPaymentStatus.Success
            : SalaryPaymentStatus.Failed;
        payment.UpdatedAt = DateTime.UtcNow;

        if (payment.Status == SalaryPaymentStatus.Success)
        {
            var advances = await _workerAdvanceRepository.GetAllAsync();
            var toDeduct = advances
                .Where(a => a.WorkerId == payment.WorkerId && !a.IsDeducted)
                .ToList();
            foreach (var adv in toDeduct)
                adv.IsDeducted = true;

            _logger.LogInformation("[MOMO] Deducted {Count} advances for WorkerId={WorkerId}",
                toDeduct.Count, payment.WorkerId);
        }

        _salaryPaymentRepository.Update(payment);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("[MOMO] Payment → {Status}", payment.Status);
        return payment.Status.ToString();
    }

    private bool Verify(string rawSignature, string signature)
    {
        var expected = ComputeHmacSha256(rawSignature, _settings.SecretKey);
        return string.Equals(expected, signature, StringComparison.OrdinalIgnoreCase);
    }

    private static string ComputeHmacSha256(string data, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    // Helpers: đọc JsonElement handle cả string và number types
    private static string GetString(JsonElement el, string key)
    {
        if (!el.TryGetProperty(key, out var v)) return "";
        return v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : v.ToString();
    }

    private static long GetLong(JsonElement el, string key)
    {
        if (!el.TryGetProperty(key, out var v)) return 0;
        return v.TryGetInt64(out var n) ? n : long.TryParse(v.GetString(), out var s) ? s : 0;
    }

    private static int GetInt(JsonElement el, string key)
    {
        if (!el.TryGetProperty(key, out var v)) return -1;
        return v.TryGetInt32(out var n) ? n : int.TryParse(v.GetString(), out var s) ? s : -1;
    }
}
