using AgriLink_DH.Share.DTOs.CropSeason;

namespace AgriLink_DH.Share.DTOs.Farm;

/// <summary>
/// Farm DTO with active crop season included
/// </summary>
public class FarmWithSeasonDto
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? AreaSize { get; set; }
    public string? AddressGps { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? GoogleMapsUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Active crop seasons for this farm
    /// </summary>
    public List<CropSeasonDto> CropSeasons { get; set; } = new();
}
