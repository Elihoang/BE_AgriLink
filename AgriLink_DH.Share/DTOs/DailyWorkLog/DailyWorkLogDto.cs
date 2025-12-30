namespace AgriLink_DH.Share.DTOs.DailyWorkLog;

public class DailyWorkLogDto
{
    public Guid Id { get; set; }
    public Guid SeasonId { get; set; }
    public DateTime WorkDate { get; set; }
    public Guid? TaskTypeId { get; set; }
    public string TaskTypeName { get; set; } = string.Empty;
    public string? Note { get; set; }
    public decimal TotalCost { get; set; }
}
