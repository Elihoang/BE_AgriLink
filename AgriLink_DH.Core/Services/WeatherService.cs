using System.Text.Json;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Share.DTOs.Weather;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgriLink_DH.Core.Services;

public class WeatherService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WeatherService> _logger;
    private readonly IFarmRepository _farmRepository;
    private readonly string _apiKey;
    private const string BaseUrl = "https://api.openweathermap.org/data/2.5";

    public WeatherService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<WeatherService> logger,
        IFarmRepository farmRepository)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _farmRepository = farmRepository;
        _apiKey = _configuration["OpenWeather:ApiKey"] ?? throw new Exception("OpenWeather API Key not configured");
    }

    /// <summary>
    /// Lấy dự báo thời tiết cho 1 farm (dựa trên tọa độ latitude/longitude)
    /// </summary>
    public async Task<WeatherForecastDto?> GetWeatherByFarmIdAsync(Guid farmId)
    {
        try
        {
            // Lấy thông tin farm để có tọa độ
            var farm = await _farmRepository.GetByIdAsync(farmId);
            if (farm == null)
            {
                _logger.LogWarning("Farm {FarmId} not found", farmId);
                return null;
            }

            decimal latitude, longitude;

            // Priority 1: Dùng Latitude/Longitude (người dùng chọn trên map)
            if (farm.Latitude.HasValue && farm.Longitude.HasValue)
            {
                latitude = farm.Latitude.Value;
                longitude = farm.Longitude.Value;
                _logger.LogInformation("Using map-selected coordinates for farm {FarmId}: {Lat}, {Lon}", 
                    farmId, latitude, longitude);
            }
            // Priority 2: Fallback - Parse từ AddressGps (backward compatibility)
            else if (!string.IsNullOrWhiteSpace(farm.AddressGps))
            {
                var coords = farm.AddressGps.Split(',', StringSplitOptions.TrimEntries);
                if (coords.Length != 2 ||
                    !decimal.TryParse(coords[0], out latitude) ||
                    !decimal.TryParse(coords[1], out longitude))
                {
                    _logger.LogWarning("Farm {FarmId} has invalid GPS format: {AddressGps}", farmId, farm.AddressGps);
                    return null;
                }
                _logger.LogInformation("Using AddressGps coordinates for farm {FarmId}: {Lat}, {Lon}", 
                    farmId, latitude, longitude);
            }
            else
            {
                _logger.LogWarning("Farm {FarmId} ({FarmName}) has no GPS coordinates (neither Lat/Lon nor AddressGps)", 
                    farmId, farm.Name);
                return null;
            }

            return await GetWeatherByCoordinatesAsync(latitude, longitude, farm.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting weather for farm {FarmId}", farmId);
            return null;
        }
    }

    /// <summary>
    /// Lấy dự báo thời tiết theo tọa độ
    /// </summary>
    public async Task<WeatherForecastDto?> GetWeatherByCoordinatesAsync(decimal latitude, decimal longitude, string? locationName = null)
    {
        try
        {
            // Gọi OpenWeather API - Current Weather
            // https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={API key}&units=metric&lang=vi
            var url = $"{BaseUrl}/weather?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric&lang=vi";

            _logger.LogInformation("Calling OpenWeather API: {Url}", url.Replace(_apiKey, "***"));

            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("OpenWeather API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var weatherData = JsonSerializer.Deserialize<OpenWeatherResponseDto>(content);

            if (weatherData == null)
            {
                _logger.LogWarning("Failed to deserialize OpenWeather response");
                return null;
            }

            // Map sang DTO đơn giản hơn
            return MapToWeatherForecastDto(weatherData, locationName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenWeather API for coordinates {Lat}, {Lon}", latitude, longitude);
            return null;
        }
    }

    /// <summary>
    /// Map từ OpenWeather response sang DTO đơn giản
    /// </summary>
    private WeatherForecastDto MapToWeatherForecastDto(OpenWeatherResponseDto data, string? locationName)
    {
        var forecast = new WeatherForecastDto
        {
            LocationName = locationName ?? data.Name,
            Latitude = data.Coord?.Lat ?? 0,
            Longitude = data.Coord?.Lon ?? 0,
            ForecastTime = DateTimeOffset.FromUnixTimeSeconds(data.Dt).UtcDateTime,
            Condition = data.Weather?.FirstOrDefault()?.Main,
            Description = data.Weather?.FirstOrDefault()?.Description,
            Temperature = data.Main?.Temp ?? 0,
            FeelsLike = data.Main?.FeelsLike ?? 0,
            TempMin = data.Main?.TempMin ?? 0,
            TempMax = data.Main?.TempMax ?? 0,
            Humidity = data.Main?.Humidity ?? 0,
            RainfallMm = data.Rain?.OneHour ?? data.Rain?.ThreeHours,
            WindSpeed = data.Wind?.Speed ?? 0,
            CloudCoverage = data.Clouds?.All ?? 0
        };

        // Thêm lời khuyên cho nông dân
        forecast.Advice = GenerateAdvice(forecast);

        return forecast;
    }

    /// <summary>
    /// Tạo lời khuyên dựa trên điều kiện thời tiết
    /// </summary>
    private string GenerateAdvice(WeatherForecastDto forecast)
    {
        var advice = new List<string>();

        // Dự báo mưa
        if (forecast.WillRain)
        {
            if (forecast.RainfallMm > 10)
            {
                advice.Add(" Mưa lớn - Tránh phun thuốc, hoãn thu hoạch");
            }
            else if (forecast.RainfallMm > 5)
            {
                advice.Add(" Mưa vừa - Nên hoãn hoạt động ngoài đồng");
            }
            else
            {
                advice.Add(" Mưa nhỏ - Có thể làm việc nhưng cẩn thận");
            }
        }

        // Nhiệt độ cao
        if (forecast.TempMax > 35)
        {
            advice.Add(" Nắng gắt - Tưới nước buổi sáng sớm hoặc chiều mát");
        }

        // Độ ẩm cao + nhiệt độ cao = dễ bệnh nấm
        if (forecast.Humidity > 80 && forecast.Temperature > 28)
        {
            advice.Add(" Độ ẩm cao - Chú ý phòng bệnh nấm");
        }

        // Gió mạnh
        if (forecast.WindSpeed > 10)
        {
            advice.Add(" Gió mạnh - Không nên phun thuốc");
        }

        return advice.Count > 0 ? string.Join(". ", advice) : " Thời tiết bình thường, phù hợp làm việc";
    }
}
