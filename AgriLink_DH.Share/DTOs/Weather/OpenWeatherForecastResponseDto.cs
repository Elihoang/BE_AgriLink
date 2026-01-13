using System.Text.Json.Serialization;

namespace AgriLink_DH.Share.DTOs.Weather;

public class OpenWeatherForecastResponseDto
{
    [JsonPropertyName("cod")]
    public string Cod { get; set; }

    [JsonPropertyName("message")]
    public int Message { get; set; }

    [JsonPropertyName("cnt")]
    public int Cnt { get; set; }

    [JsonPropertyName("list")]
    public List<ForecastItem> List { get; set; }

    [JsonPropertyName("city")]
    public CityInfo City { get; set; }
}

public class ForecastItem
{
    [JsonPropertyName("dt")]
    public long Dt { get; set; }

    [JsonPropertyName("main")]
    public Main Main { get; set; } // Reusing Main class

    [JsonPropertyName("weather")]
    public List<Weather> Weather { get; set; } // Reusing Weather class

    [JsonPropertyName("clouds")]
    public Clouds Clouds { get; set; } // Reusing Clouds class

    [JsonPropertyName("wind")]
    public Wind Wind { get; set; } // Reusing Wind class

    [JsonPropertyName("rain")]
    public Rain Rain { get; set; } // Reusing Rain class

    [JsonPropertyName("sys")]
    public ForecastSys Sys { get; set; }

    [JsonPropertyName("dt_txt")]
    public string DtTxt { get; set; }
}

public class ForecastSys
{
    [JsonPropertyName("pod")]
    public string Pod { get; set; } // "d" = day, "n" = night
}

public class CityInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("coord")]
    public Coord Coord { get; set; } // Reusing Coord class

    [JsonPropertyName("country")]
    public string Country { get; set; }
}
