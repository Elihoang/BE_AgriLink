using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Share.DTOs.PlantPosition;

public class PlantPositionDto
{
    public Guid Id { get; set; }
    public Guid FarmId { get; set; }
    public string FarmName { get; set; } = string.Empty; // From Farm.Name
    public Guid? SeasonId { get; set; }
    public string? SeasonName { get; set; } // From CropSeason.Name
    public int RowNumber { get; set; }
    public int ColumnNumber { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty; // From Product.Name
    public DateTime? PlantDate { get; set; }
    public PlantHealthStatus HealthStatus { get; set; }
    public decimal? EstimatedYield { get; set; }
    public string? Note { get; set; }
}
