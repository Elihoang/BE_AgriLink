namespace AgriLink_DH.Share.DTOs.TaskType;

public class TaskTypeDto
{
    public Guid Id { get; set; }
    public Guid FarmId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DefaultUnit { get; set; }
    public decimal? DefaultPrice { get; set; }
}
