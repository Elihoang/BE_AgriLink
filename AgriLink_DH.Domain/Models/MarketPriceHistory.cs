namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Lịch sử giá nông sản theo khu vực và thời gian
/// Lưu giá theo ngày để theo dõi biến động
/// </summary>
public class MarketPriceHistory
{
            public int Id { get; set; }
    
    /// <summary>
    /// Foreign Key đến bảng Products
    /// </summary>
            public Guid ProductId { get; set; }
    
    /// <summary>
    /// Navigation property đến Product
    /// </summary>
        public virtual Product? Product { get; set; }
    
    /// <summary>
    /// Khu vực (Đắk Lắk, Lâm Đồng, Gia Lai, Đắk Nông, ...)
    /// NULL = Toàn quốc/Trung bình
    /// </summary>
            public string? Region { get; set; }
    
    /// <summary>
    /// Mã khu vực (DAK_LAK, LAM_DONG, GIA_LAI, DAK_NONG)
    /// NULL = NATIONAL (toàn quốc)
    /// </summary>
            public string? RegionCode { get; set; }
    
    /// <summary>
    /// Giá (VND/kg hoặc VND/unit tùy ProductType)
    /// </summary>
        public decimal Price { get; set; }
    
    /// <summary>
    /// Thay đổi so với ngày hôm trước (VND)
    /// </summary>
        public decimal Change { get; set; }
    
    /// <summary>
    /// Phần trăm thay đổi
    /// </summary>
        public decimal ChangePercent { get; set; }
    
    /// <summary>
    /// Đơn vị tính (kg, tấn, quả, ...)
    /// </summary>
            public string Unit { get; set; } = "kg";
    
    /// <summary>
    /// Ngày ghi nhận giá
    /// </summary>
        public DateTime RecordedDate { get; set; }
    
    /// <summary>
    /// Nguồn dữ liệu (Admin, giacaphe.com, API, etc.)
    /// </summary>
            public string? Source { get; set; }
    
    /// <summary>
    /// Người cập nhật
    /// </summary>
            public string? UpdatedBy { get; set; }
    
    /// <summary>
    /// Thời gian tạo record
    /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Ghi chú
    /// </summary>
            public string? Notes { get; set; }
}
