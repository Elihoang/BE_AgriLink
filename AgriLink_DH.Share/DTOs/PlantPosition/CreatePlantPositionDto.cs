using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.PlantPosition;

public class CreatePlantPositionDto
{
    [Required]
    public Guid SeasonId { get; set; }

    [Required]
    [Range(1, 100)]
    public int RowNumber { get; set; }

    [Required]
    [Range(1, 100)]
    public int ColumnNumber { get; set; }

    [Required]
    public Guid ProductId { get; set; } // ID từ bảng Products (Cà phê, Sầu riêng...)

    public DateTime? PlantDate { get; set; }

    public decimal? EstimatedYield { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}
