using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Core.Validations;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.Worker;
using AgriLink_DH.Share.Extensions;

namespace AgriLink_DH.Core.Services;

public class WorkerService
{
    private readonly IWorkerRepository _workerRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IWorkAssignmentRepository _workAssignmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly WorkerValidator _validator;

    public WorkerService(
        IWorkerRepository workerRepository,
        IFarmRepository farmRepository,
        IWorkAssignmentRepository workAssignmentRepository,
        IUnitOfWork unitOfWork,
        WorkerValidator validator)
    {
        _workerRepository = workerRepository;
        _farmRepository = farmRepository;
        _workAssignmentRepository = workAssignmentRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<IEnumerable<WorkerDto>> GetWorkersBySeasonIdAsync(Guid seasonId)
    {
        var workers = await _workAssignmentRepository.GetWorkersBySeasonIdAsync(seasonId);
        return workers.Select(MapToDto);
    }

    public async Task<IEnumerable<WorkerDto>> GetWorkersByFarmIdAsync(Guid farmId)
    {
        var workers = await _workAssignmentRepository.GetWorkersByFarmIdAsync(farmId);
        return workers.Select(MapToDto);
    }

    public async Task<IEnumerable<WorkerDto>> GetAllAsync()
    {
        var workers = await _workerRepository.GetAllAsync();
        return workers.Select(MapToDto);
    }

    public async Task<WorkerDto?> GetByIdAsync(Guid id)
    {
        var worker = await _workerRepository.GetByIdAsync(id);
        return worker != null ? MapToDto(worker) : null;
    }

    public async Task<WorkerDto> CreateWorkerAsync(CreateWorkerDto dto)
    {
        var worker = new Worker(dto.FullName, dto.WorkerType, dto.Phone, dto.DefaultDailyWage, dto.ImageUrl);
        worker.UpdatePaymentInfo(dto.MomoPhone, dto.BankAccount, dto.BankName);

        await _workerRepository.AddAsync(worker);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(worker);
    }

    public async Task<WorkerDto> UpdateWorkerAsync(Guid id, UpdateWorkerDto dto)
    {
        var worker = await _workerRepository.GetByIdAsync(id);
        _validator.ValidateUpdate(worker, id);

        worker.UpdateBasicInfo(dto.FullName, dto.Phone, dto.ImageUrl);
        worker.UpdateEmploymentInfo(dto.WorkerType, dto.DefaultDailyWage);
        worker.UpdatePaymentInfo(dto.MomoPhone, dto.BankAccount, dto.BankName);
        
        if (dto.IsActive) worker.Activate();
        else worker.Deactivate();

        _workerRepository.Update(worker);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(worker);
    }

    public async Task<bool> DeleteWorkerAsync(Guid id)
    {
        var worker = await _workerRepository.GetByIdAsync(id);
        _validator.ValidateDelete(worker, id);

        _workerRepository.Remove(worker);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static WorkerDto MapToDto(Worker worker)
    {
        return new WorkerDto
        {
            Id = worker.Id,
            FullName = worker.FullName,
            Phone = worker.Phone,
            WorkerType = worker.WorkerType,
            WorkerTypeLabel = worker.WorkerType.ToVietnamese(),
            DefaultDailyWage = worker.DefaultDailyWage,
            IsActive = worker.IsActive,
            ImageUrl = worker.ImageUrl,
            MomoPhone = worker.MomoPhone,
            BankAccount = worker.BankAccount,
            BankName = worker.BankName
        };
    }
}
