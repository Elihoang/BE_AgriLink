using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.Farm;

public class UpdateFarmDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public decimal? AreaSize { get; set; }

    [MaxLength(255)]
    public string? AddressGps { get; set; }

    /// <summary>
    /// Vĩ độ - Người dùng chọn trên bản đồ
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Kinh độ - Người dùng chọn trên bản đồ
    /// </summary>
    public decimal? Longitude { get; set; }
    
    [MaxLength(500)]
    public string? GoogleMapsUrl { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }
}
