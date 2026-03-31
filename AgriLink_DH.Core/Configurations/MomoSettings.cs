namespace AgriLink_DH.Core.Configurations;

public class MomoSettings
{
    public string PartnerCode { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string QueryEndpoint { get; set; } = string.Empty;
    /// URL MoMo redirect user về sau khi thanh toán (FE page)
    public string RedirectUrl { get; set; } = string.Empty;
    /// URL MoMo gửi IPN webhook (BE endpoint) — phải public với production
    public string IpnUrl { get; set; } = string.Empty;
}
