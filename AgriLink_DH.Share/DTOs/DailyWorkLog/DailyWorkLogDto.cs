namespace AgriLink_DH.Share.DTOs.DailyWorkLog;

public class DailyWorkLogDto
{
    public Guid Id { get; set; }
    public Guid SeasonId { get; set; }
    public string? SeasonName { get; set; }
    public Guid FarmId { get; set; }
    public string? FarmName { get; set; }
    public string? ProductName { get; set; }
    public DateTime WorkDate { get; set; }
    public Guid? TaskTypeId { get; set; }
    public string TaskTypeName { get; set; } = string.Empty;
    public string? Note { get; set; }
    public decimal TotalCost { get; set; }
    public List<WorkAssignment.WorkAssignmentDto>? WorkAssignments { get; set; }
}
