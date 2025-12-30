using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.CropSeason;

public class CreateCropSeasonDto
{
    [Required]
    public Guid FarmId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Note { get; set; }
}
