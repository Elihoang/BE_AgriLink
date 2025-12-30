using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.Farm;

public class UpdateFarmDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public decimal? AreaSize { get; set; }

    [MaxLength(50)]
    public string? AddressGps { get; set; }
}
