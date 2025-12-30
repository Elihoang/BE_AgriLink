namespace AgriLink_DH.Share.DTOs.WorkerAdvance;

public class WorkerAdvanceDto
{
    public Guid Id { get; set; }
    public Guid WorkerId { get; set; }
    public string WorkerName { get; set; } = string.Empty;
    public Guid SeasonId { get; set; }
    public string SeasonName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime AdvanceDate { get; set; }
    public bool IsDeducted { get; set; }
    public string? Note { get; set; }
}
