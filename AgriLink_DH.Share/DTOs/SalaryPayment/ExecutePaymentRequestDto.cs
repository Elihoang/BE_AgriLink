namespace AgriLink_DH.Share.DTOs.SalaryPayment;

public class ExecutePaymentRequestDto
{
    public Guid WorkerId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalAdvance { get; set; }
    public decimal NetSalary { get; set; }
    public string MomoPhone { get; set; } = string.Empty;
}
