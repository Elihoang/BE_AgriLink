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
    /// [OLD ENDPOINT] Lấy dự báo đơn giản cho Farm (Compatibility)
    /// </summary>
    [HttpGet("farm/{farmId}")]
    public async Task<ActionResult<ApiResponse<WeatherForecastDto>>> GetWeatherByFarm(Guid farmId)
    {
        var forecast = await _weatherService.GetWeatherByFarmIdAsync(farmId);

        if (forecast == null)
        {
            return NotFound(ApiResponse<WeatherForecastDto>.ErrorResponse(
                "Không thể lấy dự báo thời tiết đơn giản.", 404));
        }

        return Ok(ApiResponse<WeatherForecastDto>.SuccessResponse(
            forecast,
            "Lấy dự báo thời tiết thành công"));
    }

    /// <summary>
    /// [NEW ENDPOINT] Lấy chi tiết dự báo thời tiết (Current + Hourly + Daily) cho trang Weather Page
    /// </summary>
    [HttpGet("farm/{farmId}/detail")]
    public async Task<ActionResult<ApiResponse<WeatherForecastDetailDto>>> GetWeatherDetailByFarm(Guid farmId)
    {
        var forecast = await _weatherService.GetForecastDetailAsync(farmId);

        if (forecast == null)
        {
            return NotFound(ApiResponse<WeatherForecastDetailDto>.ErrorResponse(
                "Không thể lấy dự báo thời tiết chi tiết hoặc lỗi API.", 404));
        }

        return Ok(ApiResponse<WeatherForecastDetailDto>.SuccessResponse(
            forecast,
            "Lấy dự báo thời tiết chi tiết thành công"));
    }

    /// <summary>
    /// TEST: Lấy dự báo thời tiết theo tọa độ
    /// </summary>
    [HttpGet("coordinates")]
    public async Task<ActionResult<ApiResponse<WeatherForecastDto>>> GetWeatherByCoordinates(
        [FromQuery] decimal lat,
        [FromQuery] decimal lon)
    {
        var forecast = await _weatherService.GetWeatherByCoordinatesAsync(lat, lon);

        if (forecast == null)
        {
            return NotFound(ApiResponse<WeatherForecastDto>.ErrorResponse(
                "Không thể lấy dự báo thời tiết từ API", 404));
        }

        return Ok(ApiResponse<WeatherForecastDto>.SuccessResponse(
            forecast,
            "Lấy dự báo thời tiết thành công"));
    }

    /// <summary>
    /// [NEW] Lấy danh sách các tỉnh thành hỗ trợ thời tiết (Admin only)
    /// </summary>
    [HttpGet("provinces")]
    [Authorize(Roles = "Admin")]
    public ActionResult<ApiResponse<List<ProvinceDto>>> GetProvinces()
    {
        var provinces = _weatherService.GetVietnameseProvinces();
        return Ok(ApiResponse<List<ProvinceDto>>.SuccessResponse(provinces, "Lấy danh sách tỉnh thành thành công"));
    }
}
