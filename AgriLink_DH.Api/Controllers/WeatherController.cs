using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.Weather;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly WeatherService _weatherService;

    public WeatherController(WeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    /// <summary>
    /// Lấy dự báo thời tiết cho farm (based on farm coordinates)
    /// </summary>
    [HttpGet("farm/{farmId}")]
    public async Task<ActionResult<ApiResponse<WeatherForecastDto>>> GetWeatherByFarm(Guid farmId)
    {
        var forecast = await _weatherService.GetWeatherByFarmIdAsync(farmId);

        if (forecast == null)
        {
            return NotFound(ApiResponse<WeatherForecastDto>.ErrorResponse(
                "Không thể lấy dự báo thời tiết. Kiểm tra farm có tọa độ chưa?", 404));
        }

        return Ok(ApiResponse<WeatherForecastDto>.SuccessResponse(
            forecast,
            "Lấy dự báo thời tiết thành công"));
    }

    /// <summary>
    /// TEST: Lấy dự báo thời tiết theo tọa độ (không cần farm)
    /// Ví dụ: /api/weather/coordinates?lat=10.8231&lon=106.6297 (TP.HCM)
    /// </summary>
    [HttpGet("coordinates")]
    public async Task<ActionResult<ApiResponse<WeatherForecastDto>>> GetWeatherByCoordinates(
        [FromQuery] decimal lat,
        [FromQuery] decimal lon)
    {
        if (lat < -90 || lat > 90 || lon < -180 || lon > 180)
        {
            return BadRequest(ApiResponse<WeatherForecastDto>.ErrorResponse(
                "Invalid coordinates. Lat must be [-90, 90], Lon must be [-180, 180]"));
        }

        var forecast = await _weatherService.GetWeatherByCoordinatesAsync(lat, lon);

        if (forecast == null)
        {
            return NotFound(ApiResponse<WeatherForecastDto>.ErrorResponse(
                "Không thể lấy dự báo thời tiết từ OpenWeather API", 404));
        }

        return Ok(ApiResponse<WeatherForecastDto>.SuccessResponse(
            forecast,
            "Lấy dự báo thời tiết thành công"));
    }
}
