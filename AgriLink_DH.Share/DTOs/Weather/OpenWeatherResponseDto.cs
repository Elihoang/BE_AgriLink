using System.Text.Json.Serialization;

namespace AgriLink_DH.Share.DTOs.Weather;

/// <summary>
/// Response từ OpenWeather Current Weather API
/// Docs: https://openweathermap.org/current
/// </summary>
public class OpenWeatherResponseDto
{
    [JsonPropertyName("coord")]
    public Coord? Coord { get; set; }

    [JsonPropertyName("weather")]
    public List<Weather>? Weather { get; set; }

    [JsonPropertyName("main")]
    public Main? Main { get; set; }

    [JsonPropertyName("wind")]
    public Wind? Wind { get; set; }

    [JsonPropertyName("rain")]
    public Rain? Rain { get; set; }

    [JsonPropertyName("clouds")]
    public Clouds? Clouds { get; set; }

    [JsonPropertyName("dt")]
    public long Dt { get; set; }

    [JsonPropertyName("sys")]
    public Sys? Sys { get; set; }

    [JsonPropertyName("timezone")]
    public int Timezone { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("visibility")]
    public int Visibility { get; set; }
}

public class Coord
{
    [JsonPropertyName("lon")]
    public decimal Lon { get; set; }

    [JsonPropertyName("lat")]
    public decimal Lat { get; set; }
}

public class Weather
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("main")]
    public string? Main { get; set; } // "Rain", "Clear", "Clouds"...

    [JsonPropertyName("description")]
    public string? Description { get; set; } // "light rain", "broken clouds"...

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }
}

public class Main
{
    [JsonPropertyName("temp")]
    public decimal Temp { get; set; } // Nhiệt độ (°C hoặc °F tùy unit)

    [JsonPropertyName("feels_like")]
    public decimal FeelsLike { get; set; }

    [JsonPropertyName("temp_min")]
    public decimal TempMin { get; set; }

    [JsonPropertyName("temp_max")]
    public decimal TempMax { get; set; }

    [JsonPropertyName("pressure")]
    public int Pressure { get; set; } // Áp suất (hPa)

    [JsonPropertyName("humidity")]
    public int Humidity { get; set; } // Độ ẩm (%)
}

public class Wind
{
    [JsonPropertyName("speed")]
    public decimal Speed { get; set; } // Tốc độ gió (m/s)

    [JsonPropertyName("deg")]
    public int Deg { get; set; } // Hướng gió (độ)
}

public class Rain
{
    [JsonPropertyName("1h")]
    public decimal? OneHour { get; set; } // Lượng mưa 1h qua (mm)

    [JsonPropertyName("3h")]
    public decimal? ThreeHours { get; set; } // Lượng mưa 3h qua (mm)
}

public class Clouds
{
    [JsonPropertyName("all")]
    public int All { get; set; } // Mức độ che phủ mây (%)
}

public class Sys
{
    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("sunrise")]
    public long Sunrise { get; set; }

    [JsonPropertyName("sunset")]
    public long Sunset { get; set; }
}
