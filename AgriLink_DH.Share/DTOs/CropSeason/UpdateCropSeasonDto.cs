using System.ComponentModel.DataAnnotations;
using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Share.DTOs.CropSeason;

public class UpdateCropSeasonDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public SeasonStatus? Status { get; set; }
    public string? Note { get; set; }
}
