using AgriLink_DH.Domain.Common;
using AgriLink_DH.Domain.Models.Base;
namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Lịch sử thanh toán lương qua MoMo
/// </summary>
public class SalaryPayment : BaseEntity
{
    public Guid WorkerId { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public decimal GrossSalary { get; private set; }
    public decimal TotalAdvance { get; private set; }
    public decimal NetSalary { get; private set; }

    public string? MomoOrderId { get; private set; }
    public string? MomoTransId { get; private set; }
    public int? MomoResultCode { get; private set; }
    public SalaryPaymentStatus Status { get; private set; } = SalaryPaymentStatus.Pending;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public virtual Worker Worker { get; private set; } = null!;

    protected SalaryPayment() { }

    public SalaryPayment(Guid workerId, DateTime periodStart, DateTime periodEnd, decimal grossSalary, decimal totalAdvance, decimal netSalary, string momoOrderId)
    {
        WorkerId = workerId;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        GrossSalary = grossSalary;
        TotalAdvance = totalAdvance;
        NetSalary = netSalary;
        Status = SalaryPaymentStatus.Pending;
        MomoOrderId = momoOrderId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMomoResult(string? transId, int? resultCode)
    {
        MomoTransId = transId;
        MomoResultCode = resultCode;
        
        Status = resultCode == 0 
            ? SalaryPaymentStatus.Processing 
            : SalaryPaymentStatus.Failed;
            
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsSuccess()
    {
        Status = SalaryPaymentStatus.Success;
        UpdatedAt = DateTime.UtcNow;
    }
}
