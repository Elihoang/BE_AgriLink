using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.DailyWorkLog;
using AgriLink_DH.Share.DTOs.WorkAssignment;
using AgriLink_DH.Share.Extensions;

namespace AgriLink_DH.Core.Services;

public class DailyWorkLogService
{
    private readonly IDailyWorkLogRepository _dailyWorkLogRepository;
    private readonly IWorkAssignmentRepository _workAssignmentRepository;
    private readonly ICropSeasonRepository _cropSeasonRepository;
    private readonly IWorkerRepository _workerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DailyWorkLogService(
        IDailyWorkLogRepository dailyWorkLogRepository,
        IWorkAssignmentRepository workAssignmentRepository,
        ICropSeasonRepository cropSeasonRepository,
        IWorkerRepository workerRepository,
        IUnitOfWork unitOfWork,
        RedisService redisService)
    {
        _dailyWorkLogRepository = dailyWorkLogRepository;
        _workAssignmentRepository = workAssignmentRepository;
        _cropSeasonRepository = cropSeasonRepository;
        _workerRepository = workerRepository;
        _unitOfWork = unitOfWork;
        _redisService = redisService;
    }

    private readonly RedisService _redisService;
    private const string REDIS_KEY_PREFIX = "worklogs:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    private async Task InvalidateFarmCacheAsync(Guid farmId)
    {
        // Delete all keys for this farm (ranges, seasons, etc if named correctly)
        // Pattern: worklogs:farm:{farmId}:*
        await _redisService.DeleteByPatternAsync($"{REDIS_KEY_PREFIX}farm:{farmId}:*");
    }

    private async Task<Guid> GetFarmIdBySeasonId(Guid seasonId)
    {
         var season = await _cropSeasonRepository.GetByIdAsync(seasonId);
         return season?.FarmId ?? Guid.Empty;
    }

    private async Task<Guid> GetFarmIdByLogId(Guid logId)
    {
        var log = await _dailyWorkLogRepository.GetByIdAsync(logId);
        if (log == null) return Guid.Empty;
        return await GetFarmIdBySeasonId(log.SeasonId);
    }

    public async Task<IEnumerable<DailyWorkLogDto>> GetLogsBySeasonAsync(Guid seasonId)
    {
        var logs = await _dailyWorkLogRepository.GetBySeasonIdAsync(seasonId);
        return logs.Select(MapToDto);
    }

    public async Task<IEnumerable<DailyWorkLogDto>> GetLogsByTaskTypeAsync(Guid taskTypeId)
    {
        var logs = await _dailyWorkLogRepository.GetByTaskTypeIdAsync(taskTypeId);
        return logs.Select(MapToDto);
    }

    public async Task<IEnumerable<DailyWorkLogDto>> GetLogsByFarmAndTaskTypeAsync(Guid farmId, Guid taskTypeId)
    {
        var logs = await _dailyWorkLogRepository.GetByFarmAndTaskTypeAsync(farmId, taskTypeId);
        return logs.Select(MapToDto);
    }

    public async Task<IEnumerable<DailyWorkLogDto>> GetLogsBySeasonAndTaskTypeAsync(Guid seasonId, Guid taskTypeId)
    {
        var logs = await _dailyWorkLogRepository.GetBySeasonAndTaskTypeAsync(seasonId, taskTypeId);
        return logs.Select(MapToDto);
    }

    public async Task<DailyWorkLogDto?> GetLogByIdAsync(Guid id)
    {
        var log = await _dailyWorkLogRepository.GetByIdAsync(id);
        return log == null ? null : MapToDto(log);
    }

    public async Task<IEnumerable<DailyWorkLogDto>> GetLogsByUserIdAsync(Guid userId)
    {
        var logs = await _dailyWorkLogRepository.GetByUserIdAsync(userId);
        return logs.Select(MapToDto);
    }

    public async Task<IEnumerable<DailyWorkLogDto>> GetLogsByFarmAndDateRangeAsync(Guid farmId, DateTime fromDate, DateTime toDate)
    {
        var cacheKey = $"{REDIS_KEY_PREFIX}farm:{farmId}:range:{fromDate.Ticks}-{toDate.Ticks}";
        
        var cached = await _redisService.GetAsync<List<DailyWorkLogDto>>(cacheKey);
        if (cached != null) return cached;

        var logs = await _dailyWorkLogRepository.GetByFarmAndDateRangeAsync(farmId, fromDate, toDate);
        var dtos = logs.Select(MapToDto).ToList();

        await _redisService.SetAsync(cacheKey, dtos, CacheDuration);
        return dtos;
    }

    public async Task<DailyWorkLogDto> CreateLogAsync(CreateDailyWorkLogDto dto)
    {
        var season = await _cropSeasonRepository.GetByIdAsync(dto.SeasonId);
        if (season == null)
            throw new InvalidOperationException($"Không tìm thấy vụ mùa với ID: {dto.SeasonId}");

        var log = new DailyWorkLog
        {
            SeasonId = dto.SeasonId,
            WorkDate = dto.WorkDate.ToUniversalTime(),
            TaskTypeId = dto.TaskTypeId,
            Note = dto.Note,
            TotalCost = 0
        };

        await _dailyWorkLogRepository.AddAsync(log);
        await _unitOfWork.SaveChangesAsync();

        await InvalidateFarmCacheAsync(season.FarmId);

        return MapToDto(log);
    }

    public async Task<DailyWorkLogDto> UpdateLogAsync(Guid id, UpdateDailyWorkLogDto dto)
    {
        var log = await _dailyWorkLogRepository.GetWithAssignmentsAsync(id);
        if (log == null)
            throw new KeyNotFoundException($"Không tìm thấy nhật ký làm việc với ID: {id}");

        log.WorkDate = dto.WorkDate.ToUniversalTime();
        log.TaskTypeId = dto.TaskTypeId;
        log.Note = dto.Note;

        _dailyWorkLogRepository.Update(log);
        await _unitOfWork.SaveChangesAsync();

        // Invalidate cache
        // Note: log.CropSeason might be null if not loaded. Safest to fetch or use log.SeasonId
        var farmId = log.CropSeason?.FarmId ?? await GetFarmIdBySeasonId(log.SeasonId);
        if(farmId != Guid.Empty) await InvalidateFarmCacheAsync(farmId);

        return MapToDto(log);
    }

    public async Task<bool> DeleteLogAsync(Guid id)
    {
        var log = await _dailyWorkLogRepository.GetByIdAsync(id);
        if (log == null)
            throw new KeyNotFoundException($"Không tìm thấy nhật ký làm việc với ID: {id}");

        _dailyWorkLogRepository.Remove(log);
        await _unitOfWork.SaveChangesAsync();

        var farmId = await GetFarmIdBySeasonId(log.SeasonId);
        if(farmId != Guid.Empty) await InvalidateFarmCacheAsync(farmId);

        return true;
    }

    // --- QUẢN LÝ CHẤM CÔNG (ASSIGNMENTS) ---

    public async Task<IEnumerable<WorkAssignmentDto>> GetAssignmentsByLogAsync(Guid logId)
    {
        var assignments = await _workAssignmentRepository.GetByLogIdAsync(logId);
        return assignments.Select(MapAssignmentToDto);
    }

    /// <summary>
    /// Thêm công thợ vào nhật ký.
    /// Hỗ trợ cả Công Nhật (Quantity=1) và Khoán (Quantity=số lượng gốc/kg)
    /// </summary>
    public async Task<WorkAssignmentDto> AddAssignmentAsync(CreateWorkAssignmentDto dto)
    {
        var log = await _dailyWorkLogRepository.GetByIdAsync(dto.LogId);
        if (log == null)
            throw new InvalidOperationException($"Không tìm thấy nhật ký làm việc với ID: {dto.LogId}");

        var worker = await _workerRepository.GetByIdAsync(dto.WorkerId);
        if (worker == null)
            throw new InvalidOperationException($"Không tìm thấy nhân công với ID: {dto.WorkerId}");

        // Thành tiền = Số lượng * Đơn giá
        // Công nhật: Số lượng = 1 (ngày), Đơn giá = 250k
        // Khoán: Số lượng = 50 (gốc), Đơn giá = 5k
        var totalAmount = dto.Quantity * dto.UnitPrice;

        var assignment = new WorkAssignment
        {
            LogId = dto.LogId,
            WorkerId = dto.WorkerId,
            PaymentMethod = dto.PaymentMethod,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            TotalAmount = totalAmount,
            Note = dto.Note
        };

        await _workAssignmentRepository.AddAsync(assignment);
        
        // Cập nhật tổng chi phí của nhật ký (Auto Sum)
        log.TotalCost += assignment.TotalAmount;
        _dailyWorkLogRepository.Update(log);

        await _unitOfWork.SaveChangesAsync();

        var farmId = await GetFarmIdBySeasonId(log.SeasonId);
        if(farmId != Guid.Empty) await InvalidateFarmCacheAsync(farmId);

        assignment.Worker = worker; // Populate for DTO
        return MapAssignmentToDto(assignment);
    }

    /// <summary>
    /// Thêm nhiều công thợ cùng lúc vào nhật ký (Batch Add - Tránh race condition)
    /// </summary>
    public async Task<IEnumerable<WorkAssignmentDto>> AddMultipleAssignmentsAsync(CreateMultipleAssignmentsDto dto)
    {
        var log = await _dailyWorkLogRepository.GetByIdAsync(dto.LogId);
        if (log == null)
            throw new InvalidOperationException($"Không tìm thấy nhật ký làm việc với ID: {dto.LogId}");

        var createdAssignments = new List<WorkAssignment>();
        decimal totalAddedCost = 0;

        foreach (var assignmentDto in dto.Assignments)
        {
            var worker = await _workerRepository.GetByIdAsync(assignmentDto.WorkerId);
            if (worker == null)
                throw new InvalidOperationException($"Không tìm thấy nhân công với ID: {assignmentDto.WorkerId}");

            var totalAmount = assignmentDto.Quantity * assignmentDto.UnitPrice;

            var assignment = new WorkAssignment
            {
                LogId = dto.LogId,
                WorkerId = assignmentDto.WorkerId,
                PaymentMethod = assignmentDto.PaymentMethod,
                Quantity = assignmentDto.Quantity,
                UnitPrice = assignmentDto.UnitPrice,
                TotalAmount = totalAmount,
                Note = assignmentDto.Note
            };

            assignment.Worker = worker; // Populate for DTO
            await _workAssignmentRepository.AddAsync(assignment);
            createdAssignments.Add(assignment);
            totalAddedCost += totalAmount;
        }

        // Cập nhật tổng chi phí một lần duy nhất
        log.TotalCost += totalAddedCost;
        _dailyWorkLogRepository.Update(log);

        await _unitOfWork.SaveChangesAsync();

        // Invalidate
        var farmId = await GetFarmIdBySeasonId(log.SeasonId);
        if (farmId != Guid.Empty) await InvalidateFarmCacheAsync(farmId);

        return createdAssignments.Select(MapAssignmentToDto);
    }

    public async Task<bool> RemoveAssignmentAsync(Guid assignmentId)
    {
        var assignment = await _workAssignmentRepository.GetByIdAsync(assignmentId);
        if (assignment == null)
            throw new KeyNotFoundException($"Không tìm thấy chấm công với ID: {assignmentId}");

        var log = await _dailyWorkLogRepository.GetByIdAsync(assignment.LogId);
        if (log != null)
        {
            // Trừ tiền ra khỏi tổng chi phí
            log.TotalCost -= assignment.TotalAmount;
            _dailyWorkLogRepository.Update(log);
        }

        _workAssignmentRepository.Remove(assignment);
        await _unitOfWork.SaveChangesAsync();

        if (log != null)
        {
             var farmId = await GetFarmIdBySeasonId(log.SeasonId);
             if (farmId != Guid.Empty) await InvalidateFarmCacheAsync(farmId);
        }

        return true;
    }

    /// <summary>
    /// Cập nhật chi tiết chấm công (điều chỉnh thực tế vs kế hoạch)
    /// </summary>
    public async Task<WorkAssignmentDto> UpdateAssignmentAsync(Guid assignmentId, UpdateWorkAssignmentDto dto)
    {
        var assignment = await _workAssignmentRepository.GetByIdAsync(assignmentId);
        if (assignment == null)
            throw new KeyNotFoundException($"Không tìm thấy chấm công với ID: {assignmentId}");

        var log = await _dailyWorkLogRepository.GetByIdAsync(assignment.LogId);
        if (log == null)
            throw new InvalidOperationException("Không tìm thấy nhật ký làm việc tương ứng");

        // 1. Trừ tiền cũ khỏi Log
        log.TotalCost -= assignment.TotalAmount;

        // 2. Cập nhật thông tin mới
        var newTotalAmount = dto.Quantity * dto.UnitPrice;

        assignment.PaymentMethod = dto.PaymentMethod;
        assignment.Quantity = dto.Quantity;
        assignment.UnitPrice = dto.UnitPrice;
        assignment.TotalAmount = newTotalAmount;
        assignment.Note = dto.Note;

        // 3. Cộng tiền mới vào Log
        log.TotalCost += newTotalAmount;

        _workAssignmentRepository.Update(assignment);
        _dailyWorkLogRepository.Update(log);

        await _unitOfWork.SaveChangesAsync();

        var farmId = await GetFarmIdBySeasonId(log.SeasonId);
        if (farmId != Guid.Empty) await InvalidateFarmCacheAsync(farmId);

        // Populate worker info for DTO
        if (assignment.Worker == null)
        {
            assignment.Worker = await _workerRepository.GetByIdAsync(assignment.WorkerId);
        }

        return MapAssignmentToDto(assignment);
    }

    private static DailyWorkLogDto MapToDto(DailyWorkLog log)
    {
        return new DailyWorkLogDto
        {
            Id = log.Id,
            SeasonId = log.SeasonId,
            SeasonName = log.CropSeason?.Name,
            FarmId = log.CropSeason?.FarmId ?? Guid.Empty,
            FarmName = log.CropSeason?.Farm?.Name,
            ProductName = log.CropSeason?.Product?.Name,
            WorkDate = log.WorkDate,
            TaskTypeId = log.TaskTypeId,
            TaskTypeName = log.TaskType?.Name ?? string.Empty,
            Note = log.Note,
            TotalCost = log.TotalCost,
            WorkAssignments = log.WorkAssignments?.Select(MapAssignmentToDto).ToList()
        };
    }
    public async Task<bool> SoftDeleteDailyWorkLogAsync(Guid id)
    {
        var log = await _dailyWorkLogRepository.GetByIdAsync(id);
        if (log == null)
            throw new KeyNotFoundException($"Không tìm thấy nhật ký làm việc với ID: {id}");

        log.IsDeleted = true;
        log.DeletedAt = DateTime.UtcNow;

        _dailyWorkLogRepository.Update(log);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RestoreDailyWorkLogAsync(Guid id)
    {
        var log = await _dailyWorkLogRepository.GetByIdAsync(id);
        if (log == null)
            throw new KeyNotFoundException($"Không tìm thấy nhật ký làm việc với ID: {id}");

        log.IsDeleted = false;
        log.DeletedAt = null;

        _dailyWorkLogRepository.Update(log);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
   

    private static WorkAssignmentDto MapAssignmentToDto(WorkAssignment assignment)
    {
        return new WorkAssignmentDto
        {
            Id = assignment.Id,
            LogId = assignment.LogId,
            WorkerId = assignment.WorkerId,
            WorkerName = assignment.Worker?.FullName ?? "Unknown",
            WorkerImageUrl = assignment.Worker?.ImageUrl,
            PaymentMethod = assignment.PaymentMethod,
            PaymentMethodLabel = assignment.PaymentMethod.ToVietnamese(),
            Quantity = assignment.Quantity,
            UnitPrice = assignment.UnitPrice,
            TotalAmount = assignment.TotalAmount,
            Note = assignment.Note
        };
    }
}
