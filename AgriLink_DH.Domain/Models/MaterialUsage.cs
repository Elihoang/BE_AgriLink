using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Vật tư: Phân/Thuốc - Nhật ký sử dụng vật tư
/// </summary>
public class MaterialUsage : SoftDeletableEntity
{

    public Guid SeasonId { get; private set; } // Bón cho cây nào?

    public DateTime UsageDate { get; private set; } = DateTime.UtcNow.Date;

    public string? MaterialName { get; private set; } // "NPK 16-16-8 Đầu Trâu"
    public decimal Quantity { get; private set; } // 5.5

    public string? Unit { get; private set; } // "Bao", "Lít", "Chai"
    public decimal UnitPrice { get; private set; } // Giá mua vào
    public decimal TotalCost { get; private set; } // Thành tiền

    public string? Note { get; private set; }

    // Navigation Properties
    public virtual CropSeason CropSeason { get; private set; } = null!;

    public Guid? MaterialId { get; private set; }

    public virtual Material? Material { get; private set; }

    // Soft Delete inherited from SoftDeletableEntity

    protected MaterialUsage() { }

    public MaterialUsage(Guid seasonId, DateTime usageDate, decimal quantity, decimal unitPrice, Guid? materialId = null, string? materialName = null, string? unit = null, string? note = null)
    {
        if (seasonId == Guid.Empty) throw new ArgumentException("SeasonId không hợp lệ", nameof(seasonId));
        if (quantity < 0) throw new ArgumentException("Số lượng không được âm", nameof(quantity));
        if (unitPrice < 0) throw new ArgumentException("Đơn giá không được âm", nameof(unitPrice));

        SeasonId = seasonId;
        UsageDate = usageDate.Date;
        MaterialId = materialId;
        MaterialName = materialName?.Trim();
        Quantity = quantity;
        Unit = unit?.Trim();
        UnitPrice = unitPrice;
        TotalCost = quantity * unitPrice;
        Note = note?.Trim();
    }

    public void UpdateDetails(DateTime usageDate, decimal quantity, decimal unitPrice, Guid? materialId, string? materialName, string? unit, string? note)
    {
        if (quantity < 0) throw new ArgumentException("Số lượng không được âm", nameof(quantity));
        if (unitPrice < 0) throw new ArgumentException("Đơn giá không được âm", nameof(unitPrice));

        UsageDate = usageDate.Date;
        MaterialId = materialId;
        MaterialName = materialName?.Trim();
        Quantity = quantity;
        Unit = unit?.Trim();
        UnitPrice = unitPrice;
        TotalCost = quantity * unitPrice;
        Note = note?.Trim();
    }

    // SoftDelete and Restore are inherited
}
