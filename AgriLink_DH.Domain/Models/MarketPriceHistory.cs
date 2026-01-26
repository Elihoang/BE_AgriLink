using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Lịch sử giá nông sản theo khu vực và thời gian
/// Lưu giá theo ngày để theo dõi biến động
/// </summary>
[Table("market_price_history")]
public class MarketPriceHistory
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    /// <summary>
    /// Foreign Key đến bảng Products
    /// </summary>
    [Column("product_id")]
    [Required]
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// Navigation property đến Product
    /// </summary>
    [ForeignKey("ProductId")]
    public virtual Product? Product { get; set; }
    
    /// <summary>
    /// Khu vực (Đắk Lắk, Lâm Đồng, Gia Lai, Đắk Nông, ...)
    /// NULL = Toàn quốc/Trung bình
    /// </summary>
    [Column("region")]
    [MaxLength(50)]
    public string? Region { get; set; }
    
    /// <summary>
    /// Mã khu vực (DAK_LAK, LAM_DONG, GIA_LAI, DAK_NONG)
    /// NULL = NATIONAL (toàn quốc)
    /// </summary>
    [Column("region_code")]
    [MaxLength(20)]
    public string? RegionCode { get; set; }
    
    /// <summary>
    /// Giá (VND/kg hoặc VND/unit tùy ProductType)
    /// </summary>
    [Column("price")]
    public decimal Price { get; set; }
    
    /// <summary>
    /// Thay đổi so với ngày hôm trước (VND)
    /// </summary>
    [Column("change")]
    public decimal Change { get; set; }
    
    /// <summary>
    /// Phần trăm thay đổi
    /// </summary>
    [Column("change_percent")]
    public decimal ChangePercent { get; set; }
    
    /// <summary>
    /// Đơn vị tính (kg, tấn, quả, ...)
    /// </summary>
    [Column("unit")]
    [MaxLength(20)]
    public string Unit { get; set; } = "kg";
    
    /// <summary>
    /// Ngày ghi nhận giá
    /// </summary>
    [Column("recorded_date")]
    public DateTime RecordedDate { get; set; }
    
    /// <summary>
    /// Nguồn dữ liệu (Admin, giacaphe.com, API, etc.)
    /// </summary>
    [Column("source")]
    [MaxLength(100)]
    public string? Source { get; set; }
    
    /// <summary>
    /// Người cập nhật
    /// </summary>
    [Column("updated_by")]
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }
    
    /// <summary>
    /// Thời gian tạo record
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Ghi chú
    /// </summary>
    [Column("notes")]
    [MaxLength(500)]
    public string? Notes { get; set; }
}
