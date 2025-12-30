using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Sổ Bán Hàng - Doanh thu (Tiền thực tế thu về - Cash flow)
/// </summary>
[Table("farm_sales")]
public class FarmSale
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("season_id")]
    [Required]
    public Guid SeasonId { get; set; } // Tiền của vụ nào

    [Column("sale_date")]
    public DateTime SaleDate { get; set; } = DateTime.UtcNow.Date;

    [Column("buyer_name")]
    [MaxLength(100)]
    public string? BuyerName { get; set; } // "Đại lý Tuấn", "Thương lái B"

    [Column("quantity_sold")]
    [Precision(10, 2)]
    public decimal QuantitySold { get; set; } // Bán 1000kg

    [Column("price_per_kg")]
    [Precision(15, 2)]
    public decimal PricePerKg { get; set; } // Giá 100,000

    [Column("total_revenue")]
    [Precision(15, 2)]
    public decimal TotalRevenue { get; set; } // Tổng thu: 100,000,000

    [Column("note")]
    public string? Note { get; set; } // "Tiền mặt", "Chuyển khoản", "Trừ nợ phân bón"

    // Navigation Properties
    [ForeignKey(nameof(SeasonId))]
    public virtual CropSeason CropSeason { get; set; } = null!;

    // Soft Delete
    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }
}
