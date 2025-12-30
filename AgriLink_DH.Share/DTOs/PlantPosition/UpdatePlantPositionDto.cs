using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.PlantPosition;

public class UpdatePlantPositionDto
{
    [Required]
    public Guid ProductId { get; set; }

    [MaxLength(20)]
    public string? HealthStatus { get; set; } // "healthy", "sick", "dead"

    public decimal? EstimatedYield { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}
