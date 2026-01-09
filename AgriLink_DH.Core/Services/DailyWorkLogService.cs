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
        IUnitOfWork unitOfWork)
    {
        _dailyWorkLogRepository = dailyWorkLogRepository;
        _workAssignmentRepository = workAssignmentRepository;
        _cropSeasonRepository = cropSeasonRepository;
        _workerRepository = workerRepository;
        _unitOfWork = unitOfWork;
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
        var log = await _dailyWorkLogRepository.GetWithAssignmentsAsync(id);
        return log != null ? MapToDto(log) : null;
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

        return MapToDto(log);
    }

    public async Task<bool> DeleteLogAsync(Guid id)
    {
        var log = await _dailyWorkLogRepository.GetByIdAsync(id);
        if (log == null)
            throw new KeyNotFoundException($"Không tìm thấy nhật ký làm việc với ID: {id}");

        _dailyWorkLogRepository.Remove(log);
        await _unitOfWork.SaveChangesAsync();

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

        assignment.Worker = worker; // Populate for DTO
        return MapAssignmentToDto(assignment);
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

        return true;
    }

    private static DailyWorkLogDto MapToDto(DailyWorkLog log)
    {
        return new DailyWorkLogDto
        {
            Id = log.Id,
            SeasonId = log.SeasonId,
            SeasonName = log.CropSeason?.Name,
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
            PaymentMethod = assignment.PaymentMethod,
            PaymentMethodLabel = assignment.PaymentMethod.ToVietnamese(),
            Quantity = assignment.Quantity,
            UnitPrice = assignment.UnitPrice,
            TotalAmount = assignment.TotalAmount,
            Note = assignment.Note
        };
    }
}
