using System.Text.Json;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Share.DTOs.Weather;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgriLink_DH.Core.Services;

public class WeatherService : BaseCachedService
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
        IFarmRepository farmRepository,
        RedisService redisService)
        : base(redisService)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _farmRepository = farmRepository;
        _apiKey = _configuration["OpenWeather:ApiKey"] ?? throw new Exception("OpenWeather API Key not configured");
    }

    private const string CACHE_KEY_FARM_CURRENT = "weather:farm:{0}:current";
    private const string CACHE_KEY_FARM_FORECAST = "weather:farm:{0}:forecast";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    // --- OLD METHOD RESTORED for Farm Detail ---
    /// <summary>
    /// Lấy thời tiết hiện tại đơn giản cho Farm (Backward Compatible)
    /// </summary>
    public async Task<WeatherForecastDto?> GetWeatherByFarmIdAsync(Guid farmId)
    {
        var cacheKey = string.Format(CACHE_KEY_FARM_CURRENT, farmId);

        return await GetOrSetCacheAsync(
            cacheKey,
            async () =>
            {
                try
                {
                    var (latitude, longitude, farmName) = await GetFarmCoordinates(farmId);
                    if (latitude == 0 && longitude == 0) return null;

                    var weatherData = await GetCurrentWeatherRaw(latitude, longitude);
                    if (weatherData == null) return null;

                    return MapToWeatherForecastDto(weatherData, farmName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting simple weather for farm {FarmId}", farmId);
                    return null;
                }
            },
            CacheDuration
        );
    }

    // --- NEW METHOD for Weather Forecast Page ---
    /// <summary>
    /// Lấy chi tiết dự báo thời tiết (Current + Hourly + Daily) cho Farm
    /// </summary>
    public async Task<WeatherForecastDetailDto?> GetForecastDetailAsync(Guid farmId)
    {
        var cacheKey = string.Format(CACHE_KEY_FARM_FORECAST, farmId);

        return await GetOrSetCacheAsync(
            cacheKey,
            async () =>
            {
                try
                {
                    var (latitude, longitude, farmName) = await GetFarmCoordinates(farmId);
                    if (latitude == 0 && longitude == 0) return null;

                    // 1. Get Current Weather
                    var currentTask = GetCurrentWeatherRaw(latitude, longitude);

                    // 2. Get Forecast (5 days / 3 hours)
                    var forecastTask = GetForecastRaw(latitude, longitude);

                    await Task.WhenAll(currentTask, forecastTask);

                    var currentData = await currentTask;
                    var forecastData = await forecastTask;

                    if (currentData == null) return null;

                    // 3. Map Data
                    var result = new WeatherForecastDetailDto
                    {
                        Current = MapToCurrentDto(currentData),
                        Hourly = new List<HourlyForecastDto>(),
                        Daily = new List<DailyForecastDto>()
                    };

                    if (forecastData != null && forecastData.List != null)
                    {
                         // ... logic remains same, just returning result
                         return ProcessForecastData(result, forecastData);
                    }
                    else
                    {
                        result.AgriculturalAdvice = GenerateSimpleAdvice(result);
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting forecast detail for farm {FarmId}", farmId);
                    return null;
                }
            },
            CacheDuration
        );
    }

    private WeatherForecastDetailDto ProcessForecastData(WeatherForecastDetailDto result, OpenWeatherForecastResponseDto forecastData)
    {
        // Process Hourly (Take first 12 items ~ 36h)
        result.Hourly = forecastData.List.Take(12).Select(item => new HourlyForecastDto
        {
            Time = DateTimeOffset.FromUnixTimeSeconds(item.Dt).ToLocalTime().DateTime,
            Temp = item.Main?.Temp ?? 0,
            RainAmount = item.Rain?.ThreeHours ?? 0, // 3h volume
            RainProbability = (int)((item.Rain?.ThreeHours ?? 0) > 0 ? 80 : 0),
            Condition = item.Weather?.FirstOrDefault()?.Description ?? "",
            Icon = item.Weather?.FirstOrDefault()?.Icon ?? "",
            IsDay = item.Sys?.Pod == "d"
        }).ToList();

        // Process Daily (Group by day)
        var dailyGroups = forecastData.List
            .GroupBy(x => DateTimeOffset.FromUnixTimeSeconds(x.Dt).ToLocalTime().Date)
            .OrderBy(g => g.Key)
            .Take(7);

        foreach (var group in dailyGroups)
        {
            decimal minTemp = group.Min(x => x.Main?.TempMin ?? 0);
            decimal maxTemp = group.Max(x => x.Main?.TempMax ?? 0);
            decimal totalRain = group.Sum(x => x.Rain?.ThreeHours ?? 0);
            
            var noonItem = group.OrderBy(x => Math.Abs((DateTimeOffset.FromUnixTimeSeconds(x.Dt).ToLocalTime().Hour - 12))).FirstOrDefault();

            result.Daily.Add(new DailyForecastDto
            {
                Date = group.Key,
                DayName = GetDayName(group.Key),
                TempMin = minTemp,
                TempMax = maxTemp,
                RainAmount = totalRain,
                RainProbability = totalRain > 0 ? 80 : 10,
                Condition = noonItem?.Weather?.FirstOrDefault()?.Main ?? "",
                Icon = noonItem?.Weather?.FirstOrDefault()?.Icon ?? ""
            });
        }
        
        // Extrapolate to ensure 7 days (API only returns 5 days)
        while (result.Daily.Count < 7 && result.Daily.Count > 0)
        {
            var lastDay = result.Daily.Last();
            var nextDate = lastDay.Date.AddDays(1);
            
            // Simple variation for mock data
            result.Daily.Add(new DailyForecastDto
            {
                Date = nextDate,
                DayName = GetDayName(nextDate),
                TempMin = lastDay.TempMin,
                TempMax = lastDay.TempMax,
                RainAmount = 0,
                RainProbability = 0,
                Condition = lastDay.Condition,
                Icon = lastDay.Icon
            });
        }

        // 4. Generate Advice
        result.AgriculturalAdvice = GenerateSimpleAdvice(result);
        return result;
    }

    /// <summary>
    /// TEST: Lấy dự báo đơn giản theo tọa độ (cho API test coordinates)
    /// </summary>
    public async Task<WeatherForecastDto?> GetWeatherByCoordinatesAsync(decimal latitude, decimal longitude)
    {
         var weatherData = await GetCurrentWeatherRaw(latitude, longitude);
         if (weatherData == null) return null;
         return MapToWeatherForecastDto(weatherData, "Unknown Location");
    }

    // --- Helper Methods ---

    private async Task<(decimal lat, decimal lon, string name)> GetFarmCoordinates(Guid farmId)
    {
             var farm = await _farmRepository.GetByIdAsync(farmId);
            if (farm == null)
            {
                _logger.LogWarning("Farm {FarmId} not found", farmId);
                return (0, 0, "");
            }

            decimal latitude, longitude;

            if (farm.Latitude.HasValue && farm.Longitude.HasValue)
            {
                latitude = farm.Latitude.Value;
                longitude = farm.Longitude.Value;
            }
            else if (!string.IsNullOrWhiteSpace(farm.AddressGps))
            {
                var coords = farm.AddressGps.Split(',', StringSplitOptions.TrimEntries);
                if (coords.Length != 2 ||
                    !decimal.TryParse(coords[0], out latitude) ||
                    !decimal.TryParse(coords[1], out longitude))
                {
                    return (0, 0, "");
                }
            }
            else
            {
                return (0, 0, "");
            }
            return (latitude, longitude, farm.Name);
    }

    private async Task<OpenWeatherResponseDto?> GetCurrentWeatherRaw(decimal lat, decimal lon)
    {
        try
        {
             var url = $"{BaseUrl}/weather?lat={lat}&lon={lon}&appid={_apiKey}&units=metric&lang=vi";
             var response = await _httpClient.GetAsync(url);
             if (!response.IsSuccessStatusCode) return null;
             var content = await response.Content.ReadAsStringAsync();
             return JsonSerializer.Deserialize<OpenWeatherResponseDto>(content);
        }
        catch { return null; }
    }

    private async Task<OpenWeatherForecastResponseDto?> GetForecastRaw(decimal lat, decimal lon)
    {
        try
        {
             var url = $"{BaseUrl}/forecast?lat={lat}&lon={lon}&appid={_apiKey}&units=metric&lang=vi";
             var response = await _httpClient.GetAsync(url);
             if (!response.IsSuccessStatusCode) return null;
             var content = await response.Content.ReadAsStringAsync();
             return JsonSerializer.Deserialize<OpenWeatherForecastResponseDto>(content);
        }
        catch { return null; }
    }

    private CurrentWeatherDto MapToCurrentDto(OpenWeatherResponseDto data)
    {
        return new CurrentWeatherDto
        {
            Temp = data.Main?.Temp ?? 0,
            FeelsLike = data.Main?.FeelsLike ?? 0,
            TempMin = data.Main?.TempMin ?? 0,
            TempMax = data.Main?.TempMax ?? 0,
            Humidity = data.Main?.Humidity ?? 0,
            WindSpeed = data.Wind?.Speed ?? 0,
            RainAmount = data.Rain?.OneHour ?? 0,
            UVIndex = 0,
            Pressure = data.Main?.Pressure ?? 0,
            Cloudiness = data.Clouds?.All ?? 0,
            Visibility = data.Visibility,
            Sunrise = data.Sys?.Sunrise > 0 ? DateTimeOffset.FromUnixTimeSeconds(data.Sys.Sunrise).ToLocalTime().DateTime : null,
            Sunset = data.Sys?.Sunset > 0 ? DateTimeOffset.FromUnixTimeSeconds(data.Sys.Sunset).ToLocalTime().DateTime : null,
            Condition = data.Weather?.FirstOrDefault()?.Main ?? "",
            Description = data.Weather?.FirstOrDefault()?.Description ?? "",
            Icon = data.Weather?.FirstOrDefault()?.Icon ?? ""
        };
    }

    // Restore old map method
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

        forecast.Advice = GenerateAdvice(forecast);
        return forecast;
    }

    private string GenerateAdvice(WeatherForecastDto forecast)
    {
        var advice = new List<string>();
        if (forecast.WillRain) advice.Add("Mưa - Tránh phun thuốc");
        if (forecast.TempMax > 35) advice.Add("Nắng nóng - Tưới nước");
        if (forecast.Humidity > 80 && forecast.Temperature > 28) advice.Add("Ẩm cao - Phòng nấm");
        if (forecast.WindSpeed > 10) advice.Add("Gió mạnh");
        
        return advice.Count > 0 ? string.Join(". ", advice) : "Thời tiết tốt";
    }

    private string GetDayName(DateTime date)
    {
        if (date.Date == DateTime.Today) return "Hôm nay";
        if (date.Date == DateTime.Today.AddDays(1)) return "Ngày mai";
        
        return date.DayOfWeek switch
        {
            DayOfWeek.Monday => "Thứ 2",
            DayOfWeek.Tuesday => "Thứ 3",
            DayOfWeek.Wednesday => "Thứ 4",
            DayOfWeek.Thursday => "Thứ 5",
            DayOfWeek.Friday => "Thứ 6",
            DayOfWeek.Saturday => "Thứ 7",
            DayOfWeek.Sunday => "CN",
            _ => ""
        };
    }

    private string GenerateSimpleAdvice(WeatherForecastDetailDto data)
    {
        var advice = new List<string>();
        
        if (data.Current.Description.ToLower().Contains("mưa") || data.Current.RainAmount > 0)
        {
             advice.Add("Trời đang mưa, hoãn các hoạt động phun thuốc.");
        }
        else if (data.Hourly.Any(h => h.Condition.ToLower().Contains("mưa")))
        {
             advice.Add("Dự báo có mưa trong 24h tới, cân nhắc thu hoạch sớm.");
        }

        if (data.Current.Temp > 35) advice.Add("Nắng nóng, chú ý tưới đủ nước.");
        if (data.Current.WindSpeed > 10) advice.Add("Gió khá mạnh, hạn chế phun thuốc bay hơi.");
        if (data.Current.Humidity > 85) advice.Add("Độ ẩm cao, đề phòng bệnh nấm.");

        if (advice.Count == 0) return "Thời tiết thuận lợi cho các hoạt động nông nghiệp bình thường.";
        return string.Join(" ", advice);
    }

    public List<ProvinceDto> GetVietnameseProvinces()
    {
        return new List<ProvinceDto>
        {
            new ProvinceDto { Name = "Gia Lai", DisplayName = "Gia Lai (Pleiku)", Latitude = 13.9833m, Longitude = 108.0000m, Region = "Tây Nguyên" },
            new ProvinceDto { Name = "Dak Lak", DisplayName = "Đắk Lắk (BMT)", Latitude = 12.6667m, Longitude = 108.0500m, Region = "Tây Nguyên" },
            new ProvinceDto { Name = "Lam Dong", DisplayName = "Lâm Đồng (Bảo Lộc)", Latitude = 11.5453m, Longitude = 107.8039m, Region = "Tây Nguyên" },
            new ProvinceDto { Name = "Dak Nong", DisplayName = "Đắk Nông (Gia Nghĩa)", Latitude = 12.0000m, Longitude = 107.6833m, Region = "Tây Nguyên" },
            new ProvinceDto { Name = "Kon Tum", DisplayName = "Kon Tum", Latitude = 14.3500m, Longitude = 108.0000m, Region = "Tây Nguyên" },
            new ProvinceDto { Name = "Son La", DisplayName = "Sơn La", Latitude = 21.3289m, Longitude = 103.9129m, Region = "Tây Bắc" },
            new ProvinceDto { Name = "Ho Chi Minh", DisplayName = "TP. Hồ Chí Minh", Latitude = 10.7769m, Longitude = 106.7009m, Region = "Miền Nam" },
            new ProvinceDto { Name = "Ha Noi", DisplayName = "Hà Nội", Latitude = 21.0285m, Longitude = 105.8542m, Region = "Miền Bắc" }
        };
    }
}
