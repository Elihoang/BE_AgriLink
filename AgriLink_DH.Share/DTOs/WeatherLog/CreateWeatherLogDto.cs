using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.WeatherLog;

public class CreateWeatherLogDto
{
    [Required]
    public Guid FarmId { get; set; }

    public DateTime LogDate { get; set; } = DateTime.UtcNow;

    public decimal? Temperature { get; set; }

    public decimal? Rainfall { get; set; }

    public string? Note { get; set; }
}
