using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.DailyWorkLog;
using AgriLink_DH.Share.DTOs.WorkAssignment;

namespace AgriLink_DH.Core.Validations;

public class DailyWorkLogValidator
{
    private readonly ICropSeasonRepository _cropSeasonRepository;
    private readonly IDailyWorkLogRepository _dailyWorkLogRepository;
    private readonly IWorkerRepository _workerRepository;
    private readonly IWorkAssignmentRepository _workAssignmentRepository;

    public DailyWorkLogValidator(
        ICropSeasonRepository cropSeasonRepository,
        IDailyWorkLogRepository dailyWorkLogRepository,
        IWorkerRepository workerRepository,
        IWorkAssignmentRepository workAssignmentRepository)
    {
        _cropSeasonRepository = cropSeasonRepository;
        _dailyWorkLogRepository = dailyWorkLogRepository;
        _workerRepository = workerRepository;
        _workAssignmentRepository = workAssignmentRepository;
    }

    public async Task ValidateCreateLogAsync(CreateDailyWorkLogDto dto)
    {
        var season = await _cropSeasonRepository.GetByIdAsync(dto.SeasonId);
        if (season == null)
            throw new InvalidOperationException($"Không tìm thấy vụ mùa với ID: {dto.SeasonId}");
    }

    public void ValidateUpdateLog(DailyWorkLog? log, Guid id)
    {
        if (log == null)
            throw new KeyNotFoundException($"Không tìm thấy nhật ký làm việc với ID: {id}");
    }

    public void ValidateDeleteLog(DailyWorkLog? log, Guid id)
    {
        if (log == null)
            throw new KeyNotFoundException($"Không tìm thấy nhật ký làm việc với ID: {id}");
    }

    public async Task ValidateAddAssignmentAsync(CreateWorkAssignmentDto dto)
    {
        var log = await _dailyWorkLogRepository.GetByIdAsync(dto.LogId);
        if (log == null)
            throw new InvalidOperationException($"Không tìm thấy nhật ký làm việc với ID: {dto.LogId}");

        var worker = await _workerRepository.GetByIdAsync(dto.WorkerId);
        if (worker == null)
            throw new InvalidOperationException($"Không tìm thấy nhân công với ID: {dto.WorkerId}");
    }

    public async Task ValidateAddMultipleAssignmentsAsync(CreateMultipleAssignmentsDto dto)
    {
        var log = await _dailyWorkLogRepository.GetByIdAsync(dto.LogId);
        if (log == null)
            throw new InvalidOperationException($"Không tìm thấy nhật ký làm việc với ID: {dto.LogId}");

        foreach (var assignmentDto in dto.Assignments)
        {
            var worker = await _workerRepository.GetByIdAsync(assignmentDto.WorkerId);
            if (worker == null)
                throw new InvalidOperationException($"Không tìm thấy nhân công với ID: {assignmentDto.WorkerId}");
        }
    }

    public void ValidateRemoveAssignment(WorkAssignment? assignment, Guid assignmentId)
    {
        if (assignment == null)
            throw new KeyNotFoundException($"Không tìm thấy chấm công với ID: {assignmentId}");
    }

    public async Task ValidateUpdateAssignmentAsync(WorkAssignment? assignment, Guid assignmentId)
    {
        if (assignment == null)
            throw new KeyNotFoundException($"Không tìm thấy chấm công với ID: {assignmentId}");

        var log = await _dailyWorkLogRepository.GetByIdAsync(assignment.LogId);
        if (log == null)
            throw new InvalidOperationException("Không tìm thấy nhật ký làm việc tương ứng");
    }
}
