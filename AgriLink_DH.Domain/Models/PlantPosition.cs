using AgriLink_DH.Domain.Common;

using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Vị trí từng cây trong rẫy - giống như sơ đồ ghế rạp chiếu phim
/// Mỗi record = 1 cây cụ thể tại vị trí (row, col)
/// </summary>
public class PlantPosition : BaseEntity
{

    /// <summary>
    /// Vị trí cây thuộc rẫy nào - REQUIRED
    /// Rẫy có sơ đồ cố định, cây ở vị trí này suốt đời
    /// </summary>
    public Guid FarmId { get; private set; }

    /// <summary>
    /// Vụ mùa hiện tại đang sử dụng vị trí này - OPTIONAL
    /// Null = vị trí trống hoặc cây chưa gắn vào vụ nào
    /// </summary>
    public Guid? SeasonId { get; private set; }

    /// <summary>
    /// Số hàng (row) - VD: hàng 1, 2, 3...
    /// </summary>
    public int RowNumber { get; private set; }

    /// <summary>
    /// Số cột (column) - VD: cột 1, 2, 3...
    /// </summary>
    public int ColumnNumber { get; private set; }

    /// <summary>
    /// Loại cây - Foreign Key tới bảng Products
    /// VD: ProductId của "Cà phê Arabica", "Sầu riêng Monthong"
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// Ngày trồng cây này
    /// </summary>
    public DateTime? PlantDate { get; private set; }

    /// <summary>
    /// Tình trạng sức khỏe
    /// </summary>
    public PlantHealthStatus HealthStatus { get; private set; } = PlantHealthStatus.Healthy;

    /// <summary>
    /// Năng suất ước tính (kg/năm) của cây này
    /// </summary>
    public decimal? EstimatedYield { get; private set; }

    /// <summary>
    /// Ghi chú: "Cây này bệnh vàng lá", "Thay cây mới 15/3"...
    /// </summary>
    public string? Note { get; private set; }

    // Navigation properties
    public virtual Farm Farm { get; private set; } = null!;

    public virtual CropSeason? CropSeason { get; private set; }

    public virtual Product Product { get; private set; } = null!;

    protected PlantPosition() { }

    public PlantPosition(Guid farmId, int rowNumber, int columnNumber, Guid productId, DateTime? plantDate = null)
    {
        if (farmId == Guid.Empty) throw new ArgumentException("FarmId không hợp lệ", nameof(farmId));
        if (productId == Guid.Empty) throw new ArgumentException("ProductId không hợp lệ", nameof(productId));
        if (rowNumber < 0) throw new ArgumentException("RowNumber không hợp lệ", nameof(rowNumber));
        if (columnNumber < 0) throw new ArgumentException("ColumnNumber không hợp lệ", nameof(columnNumber));

        FarmId = farmId;
        RowNumber = rowNumber;
        ColumnNumber = columnNumber;
        ProductId = productId;
        PlantDate = plantDate;
        HealthStatus = PlantHealthStatus.Healthy;
    }

    public void UpdateHealth(PlantHealthStatus newStatus, string? note = null)
    {
        HealthStatus = newStatus;
        if (note != null) Note = note.Trim();
    }

    public void UpdateDetails(Guid productId, DateTime? plantDate, decimal? estimatedYield, string? note)
    {
        if (productId == Guid.Empty) throw new ArgumentException("ProductId không hợp lệ", nameof(productId));
        
        ProductId = productId;
        PlantDate = plantDate;
        EstimatedYield = estimatedYield;
        Note = note?.Trim();
    }

    public void AssignToSeason(Guid seasonId)
    {
        if (seasonId == Guid.Empty) throw new ArgumentException("SeasonId không hợp lệ", nameof(seasonId));
        SeasonId = seasonId;
    }

    public void RemoveFromSeason()
    {
        SeasonId = null;
    }
}
