using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.WorkerAdvance;

namespace AgriLink_DH.Core.Services;

public class WorkerAdvanceService
{
    private readonly IWorkerAdvanceRepository _workerAdvanceRepository;
    private readonly IWorkerRepository _workerRepository;
    private readonly ICropSeasonRepository _cropSeasonRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WorkerAdvanceService(
        IWorkerAdvanceRepository workerAdvanceRepository,
        IWorkerRepository workerRepository,
        ICropSeasonRepository cropSeasonRepository,
        IUnitOfWork unitOfWork)
    {
        _workerAdvanceRepository = workerAdvanceRepository;
        _workerRepository = workerRepository;
        _cropSeasonRepository = cropSeasonRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<WorkerAdvanceDto>> GetAdvancesByUserIdAsync(Guid userId)
    {
        var advances = await _workerAdvanceRepository.GetByUserIdAsync(userId);
        return advances.Select(MapToDto);
    }

    public async Task<IEnumerable<WorkerAdvanceDto>> GetAdvancesByWorkerAsync(Guid workerId)
    {
        var advances = await _workerAdvanceRepository.GetByWorkerIdAsync(workerId);
        return advances.Select(MapToDto);
    }

    public async Task<IEnumerable<WorkerAdvanceDto>> GetAdvancesBySeasonAsync(Guid seasonId)
    {
        var advances = await _workerAdvanceRepository.GetBySeasonIdAsync(seasonId);
        return advances.Select(MapToDto);
    }

    public async Task<IEnumerable<WorkerAdvanceDto>> GetAdvancesByWorkerAndSeasonAsync(Guid workerId, Guid seasonId)
    {
        var advances = await _workerAdvanceRepository.GetByWorkerAndSeasonAsync(workerId, seasonId);
        return advances.Select(MapToDto);
    }

    public async Task<decimal> GetTotalAdvanceAsync(Guid workerId, Guid seasonId)
    {
        return await _workerAdvanceRepository.GetTotalAdvanceByWorkerAndSeasonAsync(workerId, seasonId);
    }

    public async Task<WorkerAdvanceDto?> GetAdvanceByIdAsync(Guid id)
    {
        var advance = await _workerAdvanceRepository.GetByIdAsync(id);
        return advance != null ? MapToDto(advance) : null;
    }

    public async Task<WorkerAdvanceDto> CreateAdvanceAsync(CreateWorkerAdvanceDto dto)
    {
        var worker = await _workerRepository.GetByIdAsync(dto.WorkerId);
        if (worker == null)
            throw new InvalidOperationException($"Không tìm thấy nhân công với ID: {dto.WorkerId}");

        var season = await _cropSeasonRepository.GetByIdAsync(dto.SeasonId);
        if (season == null)
            throw new InvalidOperationException($"Không tìm thấy vụ mùa với ID: {dto.SeasonId}");

        var advance = new WorkerAdvance
        {
            WorkerId = dto.WorkerId,
            SeasonId = dto.SeasonId,
            Amount = dto.Amount,
            AdvanceDate = dto.AdvanceDate.ToUniversalTime(),
            IsDeducted = false,
            Note = dto.Note
        };

        await _workerAdvanceRepository.AddAsync(advance);
        await _unitOfWork.SaveChangesAsync();

        advance.Worker = worker;
        advance.CropSeason = season;
        return MapToDto(advance);
    }

    public async Task<WorkerAdvanceDto> UpdateAdvanceAsync(Guid id, UpdateWorkerAdvanceDto dto)
    {
        var advance = await _workerAdvanceRepository.GetByIdAsync(id);
        if (advance == null)
            throw new KeyNotFoundException($"Không tìm thấy khoản ứng lương với ID: {id}");

        advance.Amount = dto.Amount;
        advance.AdvanceDate = dto.AdvanceDate.ToUniversalTime();
        advance.IsDeducted = dto.IsDeducted;
        advance.Note = dto.Note;

        _workerAdvanceRepository.Update(advance);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(advance);
    }

    public async Task<bool> DeleteAdvanceAsync(Guid id)
    {
        var advance = await _workerAdvanceRepository.GetByIdAsync(id);
        if (advance == null)
            throw new KeyNotFoundException($"Không tìm thấy khoản ứng lương với ID: {id}");

        if (advance.IsDeducted)
            throw new InvalidOperationException("Không thể xóa khoản ứng đã được trừ vào lương!");

        _workerAdvanceRepository.Remove(advance);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> MarkAsDeductedAsync(Guid id)
    {
        var advance = await _workerAdvanceRepository.GetByIdAsync(id);
        if (advance == null)
            throw new KeyNotFoundException($"Không tìm thấy khoản ứng lương với ID: {id}");

        advance.IsDeducted = true;
        _workerAdvanceRepository.Update(advance);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static WorkerAdvanceDto MapToDto(WorkerAdvance advance)
    {
        return new WorkerAdvanceDto
        {
            Id = advance.Id,
            WorkerId = advance.WorkerId,
            WorkerName = advance.Worker?.FullName ?? string.Empty,
            SeasonId = advance.SeasonId,
            SeasonName = advance.CropSeason?.Name ?? string.Empty,
            Amount = advance.Amount,
            AdvanceDate = advance.AdvanceDate,
            IsDeducted = advance.IsDeducted,
            Note = advance.Note,
            WorkerImageUrl = advance.Worker?.ImageUrl,
            WorkerCode = advance.WorkerId.ToString().Substring(0, 6).ToUpper()
        };
    }
}
