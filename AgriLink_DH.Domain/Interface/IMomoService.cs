namespace AgriLink_DH.Domain.Interface;

public enum MomoDisbursementResult
{
    Success,
    Failed,
    Pending
}

public class MomoDisbursementResponse
{
    public string OrderId { get; set; } = string.Empty;
    public string TransId { get; set; } = string.Empty;
    public int ResultCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public MomoDisbursementResult Status { get; set; }
}

public interface IMomoService
{
    Task<MomoDisbursementResponse> SendDisbursementAsync(string phone, decimal amount, string orderId, string orderInfo);
    Task<MomoDisbursementResponse> QueryDisbursementAsync(string orderId);
}
