using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.WeatherLog;

namespace AgriLink_DH.Core.Services;

public class WeatherLogService
{
    private readonly IWeatherLogRepository _weatherLogRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WeatherLogService(
        IWeatherLogRepository weatherLogRepository,
        IFarmRepository farmRepository,
        IUnitOfWork unitOfWork)
    {
        _weatherLogRepository = weatherLogRepository;
        _farmRepository = farmRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<WeatherLogDto>> GetByFarmAsync(Guid farmId)
    {
        var logs = await _weatherLogRepository.GetByFarmIdAsync(farmId);
        return logs.Select(MapToDto);
    }

    public async Task<IEnumerable<WeatherLogDto>> GetByDateRangeAsync(Guid farmId, DateTime startDate, DateTime endDate)
    {
        var logs = await _weatherLogRepository.GetByDateRangeAsync(farmId, startDate, endDate);
        return logs.Select(MapToDto);
    }

    public async Task<WeatherLogDto> CreateLogAsync(CreateWeatherLogDto dto)
    {
        var farm = await _farmRepository.GetByIdAsync(dto.FarmId);
        if (farm == null)
            throw new InvalidOperationException($"Không tìm thấy vườn với ID: {dto.FarmId}");

        var log = new WeatherLog
        {
            FarmId = dto.FarmId,
            LogDate = dto.LogDate.ToUniversalTime(),
            Condition = AgriLink_DH.Domain.Common.WeatherCondition.Sunny,
            RainfallMm = dto.Rainfall,
            Note = dto.Note
        };

        await _weatherLogRepository.AddAsync(log);
        await _unitOfWork.SaveChangesAsync();

        log.Farm = farm;
        return MapToDto(log);
    }

    public async Task<bool> DeleteLogAsync(Guid id)
    {
        var log = await _weatherLogRepository.GetByIdAsync(id);
        if (log == null)
            throw new KeyNotFoundException($"Không tìm thấy nhật ký thời tiết với ID: {id}");

        _weatherLogRepository.Remove(log);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static WeatherLogDto MapToDto(WeatherLog log)
    {
        return new WeatherLogDto
        {
            Id = log.Id,
            FarmId = log.FarmId,
            FarmName = log.Farm?.Name ?? string.Empty,
            LogDate = log.LogDate,
            Temperature = null,
            Rainfall = log.RainfallMm,
            Note = log.Note
        };
    }
}
