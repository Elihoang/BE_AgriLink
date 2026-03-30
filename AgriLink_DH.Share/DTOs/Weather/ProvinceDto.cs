namespace AgriLink_DH.Share.DTOs.Weather;

public class ProvinceDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Region { get; set; } = string.Empty;
}
