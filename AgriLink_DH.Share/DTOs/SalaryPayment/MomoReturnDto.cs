namespace AgriLink_DH.Share.DTOs.SalaryPayment;

/// <summary>
/// FE gửi lên sau khi MoMo redirect về /payment/result.
/// Dùng để update trạng thái payment trong DB.
/// </summary>
public class MomoReturnDto
{
    public string  OrderId    { get; set; } = string.Empty;
    public int     ResultCode { get; set; }
    public string? TransId    { get; set; }
}
