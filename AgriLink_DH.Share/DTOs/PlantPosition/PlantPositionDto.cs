namespace AgriLink_DH.Share.DTOs.PlantPosition;

public class PlantPositionDto
{
    public Guid Id { get; set; }
    public Guid SeasonId { get; set; }
    public int RowNumber { get; set; }
    public int ColumnNumber { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty; // From Product.Name
    public DateTime? PlantDate { get; set; }
    public string? HealthStatus { get; set; }
    public decimal? EstimatedYield { get; set; }
    public string? Note { get; set; }
}
