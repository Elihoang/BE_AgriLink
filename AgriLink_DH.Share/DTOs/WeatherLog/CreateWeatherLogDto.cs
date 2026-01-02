using System.ComponentModel.DataAnnotations;
using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Share.DTOs.WeatherLog;

public class CreateWeatherLogDto
{
    [Required]
    public Guid FarmId { get; set; }

    public DateTime LogDate { get; set; } = DateTime.UtcNow;

    public decimal? Temperature { get; set; }

    public decimal? Rainfall { get; set; }

    public WeatherCondition Condition { get; set; } = WeatherCondition.Sunny;

    public string? Note { get; set; }
}
