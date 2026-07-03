using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Chi tiết Bao - Lines (Cân từng bao tại rẫy)
/// </summary>
public class HarvestBagDetail : SoftDeletableEntity
{
    public Guid SessionId { get; private set; }
    public int BagIndex { get; private set; } // STT: 1, 2, 3
    public decimal GrossWeight { get; private set; } // Cân cả bì: 50.5
    public decimal Deduction { get; private set; } = 0.5m; // Trừ bì: 0.5
    public decimal NetWeight { get; private set; } // = Gross - Deduction

    // Bluetooth Scale Tracking
    public bool IsAutoWeighed { get; private set; } = false;
    public string? ScaleDeviceId { get; private set; }

    /// <summary>
    /// Bao đang ở trạng thái nháp (chưa được xác nhận lưu vào session)
    /// </summary>
    public bool IsDraft { get; private set; } = false;

    // Navigation Properties
    public virtual HarvestSession HarvestSession { get; private set; } = null!;

    // Soft Delete inherited from SoftDeletableEntity

    protected HarvestBagDetail() { }

    public HarvestBagDetail(Guid sessionId, int bagIndex, decimal grossWeight, decimal deduction = 0.5m, bool isAutoWeighed = false, string? scaleDeviceId = null, bool isDraft = false)
    {
        Id = Guid.NewGuid();
        SessionId = sessionId;
        BagIndex = bagIndex;
        GrossWeight = grossWeight;
        Deduction = deduction;
        NetWeight = grossWeight - deduction;
        IsAutoWeighed = isAutoWeighed;
        ScaleDeviceId = scaleDeviceId;
        IsDraft = isDraft;
        IsDeleted = false;
    }

    public void UpdateDetails(decimal grossWeight, decimal deduction)
    {
        GrossWeight = grossWeight;
        Deduction = deduction;
        NetWeight = grossWeight - deduction;
    }

    public void ConfirmDraft()
    {
        IsDraft = false;
    }

    // SoftDelete and Restore are inherited
}
