using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Sổ Ứng Lương - Quản lý công nợ với thợ
/// </summary>
public class WorkerAdvance : BaseEntity
{
    public Guid WorkerId { get; private set; }
    public Guid SeasonId { get; private set; } // Hạch toán vào vụ hiện tại
    public decimal Amount { get; private set; } // Số tiền ứng: 500,000
    public DateTime AdvanceDate { get; private set; } = DateTime.UtcNow.Date;
    public bool IsDeducted { get; private set; } = false; // True: Đã trừ vào lương, False: Chưa trừ
    public string? Note { get; private set; }

    // Navigation Properties
    public virtual Worker Worker { get; private set; } = null!;
    public virtual CropSeason CropSeason { get; private set; } = null!;

    protected WorkerAdvance() { }

    public WorkerAdvance(Guid workerId, Guid seasonId, decimal amount, DateTime advanceDate, string? note = null)
    {
        WorkerId = workerId;
        SeasonId = seasonId;
        Amount = amount;
        AdvanceDate = advanceDate.Date;
        IsDeducted = false;
        Note = note;
    }

    public void UpdateDetails(decimal amount, DateTime advanceDate, bool isDeducted, string? note)
    {
        Amount = amount;
        AdvanceDate = advanceDate.Date;
        IsDeducted = isDeducted;
        Note = note;
    }

    public void MarkAsDeducted()
    {
        IsDeducted = true;
    }
}
