namespace AgriLink_DH.Share.DTOs.SalaryPayment;

public class SalaryCalculationResultDto
{
    public Guid WorkerId { get; set; }
    public string WorkerName { get; set; } = string.Empty;
    public string MomoPhone { get; set; } = string.Empty;
    public decimal GrossSalary { get; set; }
    public decimal TotalAdvance { get; set; }
    public decimal NetSalary { get; set; }
    public List<Guid> AdvanceIds { get; set; } = new();
}
