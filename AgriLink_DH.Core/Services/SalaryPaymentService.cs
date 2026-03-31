using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.SalaryPayment;
using AgriLink_DH.Domain.Common;
using AgriLink_DH.Share.Extensions;

namespace AgriLink_DH.Core.Services;

public class SalaryPaymentService
{
    private readonly ISalaryPaymentRepository _salaryPaymentRepository;
    private readonly IWorkerRepository _workerRepository;
    private readonly IWorkerAdvanceRepository _workerAdvanceRepository;
    private readonly IWorkAssignmentRepository _workAssignmentRepository;
    private readonly IMomoService _momoService;
    private readonly IUnitOfWork _unitOfWork;

    public SalaryPaymentService(
        ISalaryPaymentRepository salaryPaymentRepository,
        IWorkerRepository workerRepository,
        IWorkerAdvanceRepository workerAdvanceRepository,
        IWorkAssignmentRepository workAssignmentRepository,
        IMomoService momoService,
        IUnitOfWork unitOfWork)
    {
        _salaryPaymentRepository = salaryPaymentRepository;
        _workerRepository = workerRepository;
        _workerAdvanceRepository = workerAdvanceRepository;
        _workAssignmentRepository = workAssignmentRepository;
        _momoService = momoService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<SalaryPaymentDto>> GetAllAsync()
    {
        var payments = await _salaryPaymentRepository.GetAllAsync();
        return payments.Select(MapToDto);
    }

    public async Task<SalaryCalculationResultDto> CalculateSalaryAsync(CalculateSalaryRequestDto request)
    {
        var worker = await _workerRepository.GetByIdAsync(request.WorkerId);
        if (worker == null) throw new KeyNotFoundException("Không tìm thấy nhân công.");

        // Lấy tất cả các ngày công trong khoảng thời gian
        var assignmentsForWorker = await _workAssignmentRepository.GetByWorkerIdAsync(request.WorkerId, request.PeriodStart, request.PeriodEnd);

        decimal grossSalary = assignmentsForWorker.Sum(a => a.TotalAmount);

        // Lấy tất cả tạm ứng chưa được khấu trừ
        var advances = await _workerAdvanceRepository.GetAllAsync();
        var unpaidAdvances = advances
            .Where(a => a.WorkerId == request.WorkerId && !a.IsDeducted)
            .ToList();

        decimal totalAdvance = unpaidAdvances.Sum(a => a.Amount);
        decimal netSalary = grossSalary - totalAdvance;

        return new SalaryCalculationResultDto
        {
            WorkerId = request.WorkerId,
            WorkerName = worker.FullName,
            MomoPhone = worker.MomoPhone ?? worker.Phone ?? string.Empty,
            GrossSalary = grossSalary,
            TotalAdvance = totalAdvance,
            NetSalary = netSalary > 0 ? netSalary : 0,
            AdvanceIds = unpaidAdvances.Select(a => a.Id).ToList()
        };
    }

    public async Task<SalaryPaymentDto> ExecutePaymentAsync(ExecutePaymentRequestDto request)
    {
        var worker = await _workerRepository.GetByIdAsync(request.WorkerId);
        if (worker == null) throw new KeyNotFoundException("Không tìm thấy nhân công.");

        // 1. Tạo bản ghi SalaryPayment (Pending)
        var payment = new SalaryPayment
        {
            WorkerId = request.WorkerId,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            GrossSalary = request.GrossSalary,
            TotalAdvance = request.TotalAdvance,
            NetSalary = request.NetSalary,
            Status = SalaryPaymentStatus.Pending,
            MomoOrderId = $"SALARY_{DateTime.UtcNow:yyyyMMddHHmmss}_{request.WorkerId.ToString().Substring(0, 8)}"
        };

        await _salaryPaymentRepository.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        // 2. Gọi MoMo /create → nhận payUrl để FE redirect
        var momoResponse = await _momoService.SendDisbursementAsync(
            request.MomoPhone,
            request.NetSalary,
            payment.MomoOrderId,
            $"Chi tra luong cho {worker.FullName} ky {request.PeriodStart:dd/MM} - {request.PeriodEnd:dd/MM}"
        );

        // 3. Update kết quả tạo payment URL
        payment.MomoTransId = momoResponse.TransId;
        payment.MomoResultCode = momoResponse.ResultCode;

        // resultCode==0 từ /create = "URL tạo thành công, đang chờ user thanh toán"
        // Advance chỉ deduct khi IPN callback về với resultCode==0
        payment.Status = momoResponse.ResultCode == 0
            ? SalaryPaymentStatus.Processing
            : SalaryPaymentStatus.Failed;

        payment.UpdatedAt = DateTime.UtcNow;

        _salaryPaymentRepository.Update(payment);
        await _unitOfWork.SaveChangesAsync();

        // Trả về DTO kèm payUrl — FE dùng để redirect sang MoMo
        var dto = MapToDto(payment);
        dto.MomoPayUrl = momoResponse.PayUrl;
        return dto;
    }

    private SalaryPaymentDto MapToDto(SalaryPayment sp)
    {
        return new SalaryPaymentDto
        {
            Id = sp.Id,
            WorkerId = sp.WorkerId,
            WorkerName = sp.Worker?.FullName ?? "Unknown",
            PeriodStart = sp.PeriodStart,
            PeriodEnd = sp.PeriodEnd,
            GrossSalary = sp.GrossSalary,
            TotalAdvance = sp.TotalAdvance,
            NetSalary = sp.NetSalary,
            MomoOrderId = sp.MomoOrderId,
            MomoTransId = sp.MomoTransId,
            MomoResultCode = sp.MomoResultCode,
            Status = sp.Status,
            StatusLabel = GetStatusLabel(sp.Status),
            CreatedAt = sp.CreatedAt
        };
    }

    private string GetStatusLabel(SalaryPaymentStatus status)
    {
        return status switch
        {
            SalaryPaymentStatus.Pending => "Chờ xử lý",
            SalaryPaymentStatus.Processing => "Đang thanh toán",
            SalaryPaymentStatus.Success => "Thành công",
            SalaryPaymentStatus.Failed => "Thất bại",
            _ => "Không xác định"
        };
    }
}
