namespace AgriLink_DH.Share.DTOs.Weather;

public class WeatherForecastDetailDto
{
    public CurrentWeatherDto Current { get; set; }
    public List<HourlyForecastDto> Hourly { get; set; } = new();
    public List<DailyForecastDto> Daily { get; set; } = new();
    public string AgriculturalAdvice { get; set; }
}

public class CurrentWeatherDto
{
    public decimal Temp { get; set; }
    public decimal FeelsLike { get; set; }
    public decimal TempMin { get; set; }
    public decimal TempMax { get; set; }
    public int Humidity { get; set; } // API returns int
    public decimal WindSpeed { get; set; }
    public decimal RainAmount { get; set; } // mm
    public decimal UVIndex { get; set; }
    public int Pressure { get; set; } // hPa
    public int Cloudiness { get; set; } // %
    public int Visibility { get; set; } // meters
    public DateTime? Sunrise { get; set; }
    public DateTime? Sunset { get; set; }
    public string Condition { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
}

public class HourlyForecastDto
{
    public DateTime Time { get; set; }
    public decimal Temp { get; set; }
    public int RainProbability { get; set; } // %
    public decimal RainAmount { get; set; } // mm
    public string Condition { get; set; }
    public string Icon { get; set; }
    public bool IsDay { get; set; }
}

public class DailyForecastDto
{
    public DateTime Date { get; set; }
    public decimal TempMax { get; set; }
    public decimal TempMin { get; set; }
    public int RainProbability { get; set; }
    public decimal RainAmount { get; set; }
    public string Condition { get; set; }
    public string Icon { get; set; }
    public string DayName { get; set; } // "Thứ 2", "Hôm nay"
}
