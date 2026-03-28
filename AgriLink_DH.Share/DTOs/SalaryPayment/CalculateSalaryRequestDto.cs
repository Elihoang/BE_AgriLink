namespace AgriLink_DH.Share.DTOs.SalaryPayment;

public class CalculateSalaryRequestDto
{
    public Guid WorkerId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}
