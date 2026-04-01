using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Share.DTOs.SalaryPayment;

public class SalaryPaymentDto
{
    public Guid Id { get; set; }
    public Guid WorkerId { get; set; }
    public string WorkerName { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalAdvance { get; set; }
    public decimal NetSalary { get; set; }
    public string? MomoOrderId { get; set; }
    public string? MomoTransId { get; set; }
    public string? MomoPayUrl { get; set; }  // URL redirect MoMo — chỉ có lúc vừa tạo mới
    public int? MomoResultCode { get; set; }
    public SalaryPaymentStatus Status { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
