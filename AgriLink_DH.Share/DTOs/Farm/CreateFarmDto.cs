using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.Farm;

public class CreateFarmDto
{
    [Required(ErrorMessage = "Tên vườn là bắt buộc")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public decimal? AreaSize { get; set; }

    [MaxLength(50)]
    public string? AddressGps { get; set; }
}
