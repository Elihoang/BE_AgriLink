using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Nhật ký công việc tại Vườn - Header (Ghi nhận: Hôm nay tại Vườn A có hoạt động gì?)
/// </summary>
public class DailyWorkLog : SoftDeletableEntity
{

    public Guid SeasonId { get; private set; } // Gắn việc vào Vụ/Vườn cụ thể

    public DateTime WorkDate { get; private set; } = DateTime.UtcNow.Date;

    public Guid? TaskTypeId { get; private set; } // Link tới loại công việc

    public string? Note { get; private set; }
    public decimal TotalCost { get; private set; } = 0; // Tổng chi phí trong ngày cho đầu việc này (Tự động cộng dồn)

    // Navigation Properties
    public virtual CropSeason CropSeason { get; private set; } = null!;

    public virtual TaskType? TaskType { get; private set; }

    public virtual ICollection<WorkAssignment> WorkAssignments { get; private set; } = new List<WorkAssignment>();

    // Soft Delete inherited from SoftDeletableEntity

    protected DailyWorkLog() { }

    public DailyWorkLog(Guid seasonId, DateTime workDate, Guid? taskTypeId = null, string? note = null)
    {
        if (seasonId == Guid.Empty) throw new ArgumentException("SeasonId không hợp lệ", nameof(seasonId));

        SeasonId = seasonId;
        WorkDate = workDate.Date;
        TaskTypeId = taskTypeId;
        Note = note?.Trim();
        TotalCost = 0;
    }

    public void UpdateDetails(DateTime workDate, Guid? taskTypeId, string? note)
    {
        WorkDate = workDate.Date;
        TaskTypeId = taskTypeId;
        Note = note?.Trim();
    }

    public void CalculateTotalCost()
    {
        TotalCost = WorkAssignments.Sum(wa => wa.TotalAmount);
    }

    public void AddCost(decimal amount)
    {
        if (amount < 0) throw new ArgumentException("Số tiền không được âm", nameof(amount));
        TotalCost += amount;
    }

    public void SubtractCost(decimal amount)
    {
        if (amount < 0) throw new ArgumentException("Số tiền không được âm", nameof(amount));
        TotalCost -= amount;
        if (TotalCost < 0) TotalCost = 0;
    }

    // SoftDelete and Restore are inherited
}
