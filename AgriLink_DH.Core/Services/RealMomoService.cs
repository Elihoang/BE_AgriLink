using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgriLink_DH.Core.Configurations;
using AgriLink_DH.Domain.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AgriLink_DH.Core.Services;

/// <summary>
/// Tích hợp MoMo Payment Gateway (/v2/gateway/api/create).
/// Flow: BE tạo request → MoMo trả về payUrl → FE redirect user sang MoMo để thanh toán.
/// </summary>
public class RealMomoService : IMomoService
{
    private readonly MomoSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<RealMomoService> _logger;

    public RealMomoService(
        IOptions<MomoSettings> settings,
        HttpClient httpClient,
        ILogger<RealMomoService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<MomoDisbursementResponse> SendDisbursementAsync(
        string phone, decimal amount, string orderId, string orderInfo)
    {
        var requestId = Guid.NewGuid().ToString("N");
        var requestType = "captureWallet"; // MoMo payment gateway standard
        var amountLong = (long)amount;
        var redirectUrl = _settings.RedirectUrl; // FE page sau khi user thanh toán xong
        var ipnUrl = _settings.IpnUrl;           // BE webhook MoMo gọi để cập nhật trạng thái
        var extraData = "";

        // Build signature theo MoMo /create spec (alphabetical key order)
        var rawSignature =
            $"accessKey={_settings.AccessKey}" +
            $"&amount={amountLong}" +
            $"&extraData={extraData}" +
            $"&ipnUrl={ipnUrl}" +
            $"&orderId={orderId}" +
            $"&orderInfo={orderInfo}" +
            $"&partnerCode={_settings.PartnerCode}" +
            $"&redirectUrl={redirectUrl}" +
            $"&requestId={requestId}" +
            $"&requestType={requestType}";

        var signature = ComputeHmacSha256(rawSignature, _settings.SecretKey);

        var payload = new
        {
            partnerCode = _settings.PartnerCode,
            requestId,
            amount = amountLong,
            orderId,
            orderInfo,
            redirectUrl,
            ipnUrl,
            requestType,
            lang = "vi",
            extraData,
            signature
        };

        _logger.LogInformation(
            "[MOMO] CreatePayment → OrderId={OrderId} | Phone={Phone} | Amount={Amount}",
            orderId, phone, amountLong);

        try
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpResponse = await _httpClient.PostAsync(_settings.Endpoint, content);
            var responseBody = await httpResponse.Content.ReadAsStringAsync();

            _logger.LogInformation("[MOMO] Response [{StatusCode}]: {Body}",
                (int)httpResponse.StatusCode, responseBody);

            var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
            var resultCode = result.GetProperty("resultCode").GetInt32();
            var message = result.TryGetProperty("message", out var m) ? m.GetString() ?? "" : "";
            // payUrl: URL MoMo cấp để redirect user đến trang thanh toán
            var payUrl = result.TryGetProperty("payUrl", out var p) ? p.GetString() ?? "" : "";

            return new MomoDisbursementResponse
            {
                OrderId = orderId,
                TransId = result.TryGetProperty("transId", out var t) ? t.ToString() : "",
                PayUrl = payUrl,
                ResultCode = resultCode,
                Message = message,
                Status = resultCode == 0
                    ? MomoDisbursementResult.Success
                    : MomoDisbursementResult.Failed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MOMO] CreatePayment FAILED: {Message}", ex.Message);
            return new MomoDisbursementResponse
            {
                OrderId = orderId,
                ResultCode = -1,
                Message = $"Lỗi kết nối MoMo: {ex.Message}",
                Status = MomoDisbursementResult.Failed
            };
        }
    }

    public async Task<MomoDisbursementResponse> QueryDisbursementAsync(string orderId)
    {
        var requestId = Guid.NewGuid().ToString("N");

        var rawSignature =
            $"accessKey={_settings.AccessKey}" +
            $"&orderId={orderId}" +
            $"&partnerCode={_settings.PartnerCode}" +
            $"&requestId={requestId}";

        var signature = ComputeHmacSha256(rawSignature, _settings.SecretKey);

        var payload = new
        {
            partnerCode = _settings.PartnerCode,
            requestId,
            orderId,
            lang = "vi",
            signature
        };

        var queryEndpoint = string.IsNullOrEmpty(_settings.QueryEndpoint)
            ? _settings.Endpoint.Replace("/create", "/query")
            : _settings.QueryEndpoint;

        _logger.LogInformation("[MOMO] QueryPayment → OrderId={OrderId}", orderId);

        try
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpResponse = await _httpClient.PostAsync(queryEndpoint, content);
            var responseBody = await httpResponse.Content.ReadAsStringAsync();

            _logger.LogInformation("[MOMO] Query Response [{StatusCode}]: {Body}",
                (int)httpResponse.StatusCode, responseBody);

            var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
            var resultCode = result.GetProperty("resultCode").GetInt32();

            return new MomoDisbursementResponse
            {
                OrderId = orderId,
                TransId = result.TryGetProperty("transId", out var t) ? t.ToString() : "",
                ResultCode = resultCode,
                Message = result.TryGetProperty("message", out var m) ? m.GetString() ?? "" : "",
                Status = resultCode == 0
                    ? MomoDisbursementResult.Success
                    : MomoDisbursementResult.Failed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MOMO] QueryPayment FAILED: {Message}", ex.Message);
            return new MomoDisbursementResponse
            {
                OrderId = orderId,
                ResultCode = -1,
                Message = $"Lỗi query MoMo: {ex.Message}",
                Status = MomoDisbursementResult.Failed
            };
        }
    }

    private static string ComputeHmacSha256(string data, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}
