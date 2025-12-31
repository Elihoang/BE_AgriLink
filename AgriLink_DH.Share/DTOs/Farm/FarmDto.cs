namespace AgriLink_DH.Share.DTOs.Farm;

public class FarmDto
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? AreaSize { get; set; }
    public string? AddressGps { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
}
