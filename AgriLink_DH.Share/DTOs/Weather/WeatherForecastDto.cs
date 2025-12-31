namespace AgriLink_DH.Share.DTOs.Weather;

/// <summary>
/// DTO đơn giản hóa để trả về app - dễ đọc hơn
/// </summary>
public class WeatherForecastDto
{
    /// <summary>
    /// Tên địa điểm
    /// </summary>
    public string? LocationName { get; set; }

    /// <summary>
    /// Tọa độ
    /// </summary>
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    /// <summary>
    /// Thời gian dự báo
    /// </summary>
    public DateTime ForecastTime { get; set; }

    /// <summary>
    /// Điều kiện thời tiết (Rain, Clear, Clouds...)
    /// </summary>
    public string? Condition { get; set; }

    /// <summary>
    /// Mô tả chi tiết (light rain, heavy rain...)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Nhiệt độ hiện tại (°C)
    /// </summary>
    public decimal Temperature { get; set; }

    /// <summary>
    /// Cảm giác như (°C)
    /// </summary>
    public decimal FeelsLike { get; set; }

    /// <summary>
    /// Nhiệt độ tối thiểu (°C)
    /// </summary>
    public decimal TempMin { get; set; }

    /// <summary>
    /// Nhiệt độ tối đa (°C)
    /// </summary>
    public decimal TempMax { get; set; }

    /// <summary>
    /// Độ ẩm (%)
    /// </summary>
    public int Humidity { get; set; }

    /// <summary>
    /// Lượng mưa dự kiến (mm) - 1h qua hoặc 3h qua
    /// </summary>
    public decimal? RainfallMm { get; set; }

    /// <summary>
    /// Tốc độ gió (m/s)
    /// </summary>
    public decimal WindSpeed { get; set; }

    /// <summary>
    /// Mức độ mây che phủ (%)
    /// </summary>
    public int CloudCoverage { get; set; }

    /// <summary>
    /// Có mưa không?
    /// </summary>
    public bool WillRain => Condition?.ToLower().Contains("rain") == true || RainfallMm > 0;

    /// <summary>
    /// Lời khuyên cho nông dân
    /// </summary>
    public string? Advice { get; set; }
}
