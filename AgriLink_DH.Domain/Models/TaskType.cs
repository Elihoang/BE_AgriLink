using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Đơn giá Công việc Mẫu - Giúp nhập liệu nhanh
/// </summary>
public class TaskType : BaseEntity
{

    public Guid? FarmId { get; private set; }

    public bool IsSystem { get; private set; } = false;

    public string Name { get; private set; } = string.Empty; // "Làm cành", "Hái khoán", "Bón phân"

    public string? DefaultUnit { get; private set; } // 'CONG' (Ngày), 'KG', 'GOC' (Gốc)
    
    public decimal? DefaultPrice { get; private set; } // Giá gợi ý. VD: 350000 hoặc 1200

    // Navigation Properties
    public virtual Farm? Farm { get; private set; }

    protected TaskType() { }

    public TaskType(string name, Guid? farmId = null, bool isSystem = false, string? defaultUnit = null, decimal? defaultPrice = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên loại công việc không được để trống", nameof(name));
            
        Name = name.Trim();
        FarmId = farmId;
        IsSystem = isSystem;
        DefaultUnit = defaultUnit?.Trim();
        DefaultPrice = defaultPrice;
    }

    public void UpdateDetails(string name, string? defaultUnit, decimal? defaultPrice)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên loại công việc không được để trống", nameof(name));

        if (IsSystem)
            throw new InvalidOperationException("Không thể sửa loại công việc của hệ thống");

        Name = name.Trim();
        DefaultUnit = defaultUnit?.Trim();
        DefaultPrice = defaultPrice;
    }
}
