using AgriLink_DH.Domain.Common;

using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Bảng Chấm Công Chi Tiết - Lines (Ông Tèo hôm nay làm ở đây bao lâu, bao nhiêu tiền?)
/// </summary>
public class WorkAssignment : BaseEntity
{

    public Guid LogId { get; private set; } // Link tới Nhật ký ở trên

    public Guid WorkerId { get; private set; } // Link tới ông Tèo

    public PaymentMethod PaymentMethod { get; private set; } // 'DAILY', 'PRODUCT'
    public decimal Quantity { get; private set; } // 0.5 (công), 200 (kg)
    public decimal UnitPrice { get; private set; } // Đơn giá (VD: 250k/công)
    public decimal TotalAmount { get; private set; } // Thành tiền (= quantity * unit_price)

    public string? Note { get; private set; }

    // Navigation Properties
    public virtual DailyWorkLog DailyWorkLog { get; private set; } = null!;

    public virtual Worker Worker { get; private set; } = null!;

    protected WorkAssignment() { }

    public WorkAssignment(Guid logId, Guid workerId, PaymentMethod paymentMethod, decimal quantity, decimal unitPrice, string? note = null)
    {
        if (logId == Guid.Empty) throw new ArgumentException("LogId không hợp lệ", nameof(logId));
        if (workerId == Guid.Empty) throw new ArgumentException("WorkerId không hợp lệ", nameof(workerId));
        if (quantity < 0) throw new ArgumentException("Số lượng không được âm", nameof(quantity));
        if (unitPrice < 0) throw new ArgumentException("Đơn giá không được âm", nameof(unitPrice));

        LogId = logId;
        WorkerId = workerId;
        PaymentMethod = paymentMethod;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalAmount = quantity * unitPrice;
        Note = note?.Trim();
    }

    public void UpdateDetails(PaymentMethod paymentMethod, decimal quantity, decimal unitPrice, string? note)
    {
        if (quantity < 0) throw new ArgumentException("Số lượng không được âm", nameof(quantity));
        if (unitPrice < 0) throw new ArgumentException("Đơn giá không được âm", nameof(unitPrice));

        PaymentMethod = paymentMethod;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalAmount = quantity * unitPrice;
        Note = note?.Trim();
    }
}
