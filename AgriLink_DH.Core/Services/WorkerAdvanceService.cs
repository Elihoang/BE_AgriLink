using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Core.Validations;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.WorkerAdvance;

namespace AgriLink_DH.Core.Services;

public class WorkerAdvanceService
{
    private readonly IWorkerAdvanceRepository _workerAdvanceRepository;
    private readonly IWorkerRepository _workerRepository;
    private readonly ICropSeasonRepository _cropSeasonRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly WorkerAdvanceValidator _validator;

    public WorkerAdvanceService(
        IWorkerAdvanceRepository workerAdvanceRepository,
        IWorkerRepository workerRepository,
        ICropSeasonRepository cropSeasonRepository,
        IUnitOfWork unitOfWork,
        WorkerAdvanceValidator validator)
    {
        _workerAdvanceRepository = workerAdvanceRepository;
        _workerRepository = workerRepository;
        _cropSeasonRepository = cropSeasonRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
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
        var season = await _cropSeasonRepository.GetByIdAsync(dto.SeasonId);

        _validator.ValidateCreate(worker, season);

        var advance = new WorkerAdvance(dto.WorkerId, dto.SeasonId, dto.Amount, dto.AdvanceDate.ToUniversalTime(), dto.Note);

        await _workerAdvanceRepository.AddAsync(advance);
        await _unitOfWork.SaveChangesAsync();

        var resultDto = MapToDto(advance);
        resultDto.WorkerName = worker.FullName;
        resultDto.SeasonName = season.Name;
        resultDto.WorkerImageUrl = worker.ImageUrl;
        return resultDto;
    }

    public async Task<WorkerAdvanceDto> UpdateAdvanceAsync(Guid id, UpdateWorkerAdvanceDto dto)
    {
        var advance = await _workerAdvanceRepository.GetByIdAsync(id);
        if (advance == null)
            throw new KeyNotFoundException($"Không tìm thấy khoản ứng lương với ID: {id}");

        advance.UpdateDetails(dto.Amount, dto.AdvanceDate.ToUniversalTime(), dto.IsDeducted, dto.Note);

        _workerAdvanceRepository.Update(advance);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(advance);
    }

    public async Task<bool> DeleteAdvanceAsync(Guid id)
    {
        var advance = await _workerAdvanceRepository.GetByIdAsync(id);
        _validator.ValidateDelete(advance);

        _workerAdvanceRepository.Remove(advance);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> MarkAsDeductedAsync(Guid id)
    {
        var advance = await _workerAdvanceRepository.GetByIdAsync(id);
        if (advance == null)
            throw new KeyNotFoundException($"Không tìm thấy khoản ứng lương với ID: {id}");

        advance.MarkAsDeducted();
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
