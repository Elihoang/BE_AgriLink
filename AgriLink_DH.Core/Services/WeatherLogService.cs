using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Core.Validations;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Domain.Common;
using AgriLink_DH.Share.DTOs.WeatherLog;

namespace AgriLink_DH.Core.Services;

public class WeatherLogService
{
    private readonly IWeatherLogRepository _weatherLogRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly WeatherLogValidator _validator;

    public WeatherLogService(
        IWeatherLogRepository weatherLogRepository,
        IFarmRepository farmRepository,
        IUnitOfWork unitOfWork,
        WeatherLogValidator validator)
    {
        _weatherLogRepository = weatherLogRepository;
        _farmRepository = farmRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
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

    public async Task<IEnumerable<WeatherLogDto>> GetByUserIdAsync(Guid userId)
    {
        var logs = await _weatherLogRepository.GetByUserIdAsync(userId);
        return logs.Select(MapToDto);
    }

    public async Task<WeatherLogDto> CreateLogAsync(CreateWeatherLogDto dto)
    {
        await _validator.ValidateCreateLogAsync(dto.FarmId);
        var farm = (await _farmRepository.GetByIdAsync(dto.FarmId))!;

        var log = new WeatherLog
        {
            FarmId = dto.FarmId,
            LogDate = dto.LogDate.ToUniversalTime(),
            Condition = dto.Condition,
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
        _validator.ValidateDeleteLog(log, id);

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
            Condition = log.Condition.ToString(),
            ConditionLabel = GetConditionLabel(log.Condition),
            Temperature = null,
            Rainfall = log.RainfallMm,
            Note = log.Note
        };
    }

    private static string GetConditionLabel(WeatherCondition condition)
    {
        return condition switch
        {
            WeatherCondition.Sunny => "Nắng",
            WeatherCondition.Rainy => "Mưa",
            WeatherCondition.Cloudy => "Nhiều mây",
            WeatherCondition.Storm => "Giông bão",
            _ => "Không xác định"
        };
    }
}
