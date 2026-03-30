namespace AgriLink_DH.Core.Configurations;

public class MomoSettings
{
    public string PartnerCode { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty; // Required for RSA encryption of payToken if using actual Disbursement
    public string Endpoint { get; set; } = string.Empty;
}
