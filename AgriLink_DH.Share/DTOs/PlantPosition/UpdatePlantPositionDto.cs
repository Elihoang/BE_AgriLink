using System.ComponentModel.DataAnnotations;
using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Share.DTOs.PlantPosition;

public class UpdatePlantPositionDto
{
    [Required]
    public Guid ProductId { get; set; }

    /// <summary>
    /// Tình trạng sức khỏe của cây
    /// </summary>
    public PlantHealthStatus? HealthStatus { get; set; }

    public decimal? EstimatedYield { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}
