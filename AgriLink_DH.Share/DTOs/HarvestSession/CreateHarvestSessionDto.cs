using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.HarvestSession;

public class CreateHarvestSessionDto
{
    [Required]
    public Guid SeasonId { get; set; }

    public DateTime HarvestDate { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? StorageLocation { get; set; }
}
