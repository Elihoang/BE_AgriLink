using AgriLink_DH.Domain.Common;
using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Danh sách Nhân công - Quản lý hồ sơ người làm
/// </summary>
public class Worker : BaseEntity
{

    public string FullName { get; private set; } = string.Empty; // "Chú Bảy", "Tèo"

    public string? Phone { get; private set; }

    public WorkerType WorkerType { get; private set; } = WorkerType.Seasonal;
    public decimal? DefaultDailyWage { get; private set; } // Lương ngày mặc định

    public bool IsActive { get; private set; } = true; // False nếu đã nghỉ việc

    public string? ImageUrl { get; private set; } // URL hình ảnh nhân công

    public string? MomoPhone { get; private set; } // SĐT MoMo riêng (nếu khác SĐT thường)

    public string? BankAccount { get; private set; }

    public string? BankName { get; private set; }

    // Navigation Properties
    // No Farm Navigation property needed here as it belongs to User

    public virtual ICollection<WorkAssignment> WorkAssignments { get; private set; } = new List<WorkAssignment>();
    public virtual ICollection<WorkerAdvance> WorkerAdvances { get; private set; } = new List<WorkerAdvance>();
    public virtual ICollection<SalaryPayment> SalaryPayments { get; private set; } = new List<SalaryPayment>();

    protected Worker() { }

    public Worker(string fullName, WorkerType workerType, string? phone = null, decimal? defaultDailyWage = null, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Tên nhân công không được để trống", nameof(fullName));

        FullName = fullName.Trim();
        WorkerType = workerType;
        Phone = phone?.Trim();
        DefaultDailyWage = defaultDailyWage;
        ImageUrl = imageUrl?.Trim();
        IsActive = true;
    }

    public void UpdateBasicInfo(string fullName, string? phone, string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Tên nhân công không được để trống", nameof(fullName));

        FullName = fullName.Trim();
        Phone = phone?.Trim();
        if (imageUrl != null) ImageUrl = imageUrl.Trim();
    }

    public void UpdateEmploymentInfo(WorkerType workerType, decimal? defaultDailyWage)
    {
        WorkerType = workerType;
        DefaultDailyWage = defaultDailyWage;
    }

    public void UpdatePaymentInfo(string? momoPhone, string? bankAccount, string? bankName)
    {
        MomoPhone = momoPhone?.Trim();
        BankAccount = bankAccount?.Trim();
        BankName = bankName?.Trim();
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
