using AgriLink_DH.Domain.Interface;

namespace AgriLink_DH.Core.Services;

public class MockMomoService : IMomoService
{
    public async Task<MomoDisbursementResponse> SendDisbursementAsync(string phone, decimal amount, string orderId, string orderInfo)
    {
        // Giả lập xử lý thanh toán MoMo
        await Task.Delay(1000);

        return new MomoDisbursementResponse
        {
            OrderId = orderId,
            TransId = "MOCK_" + Guid.NewGuid().ToString("N").Substring(0, 10),
            ResultCode = 0, // Success
            Message = "Thanh toán thành công (MOCK)",
            Status = MomoDisbursementResult.Success
        };
    }

    public async Task<MomoDisbursementResponse> QueryDisbursementAsync(string orderId)
    {
        await Task.Delay(500);
        return new MomoDisbursementResponse
        {
            OrderId = orderId,
            TransId = "MOCK_QUERY_" + Guid.NewGuid().ToString("N").Substring(0, 10),
            ResultCode = 0,
            Message = "Truy vấn thành công (MOCK)",
            Status = MomoDisbursementResult.Success
        };
    }
}
