namespace AgriLink_DH.Share.DTOs.WeatherLog;

public class WeatherLogDto
{
    public Guid Id { get; set; }
    public Guid FarmId { get; set; }
    public string FarmName { get; set; } = string.Empty;
    public DateTime LogDate { get; set; }
    public decimal? Temperature { get; set; }
    public decimal? Rainfall { get; set; }
    public string? Note { get; set; }
}
