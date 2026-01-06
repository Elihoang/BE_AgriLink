using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Vật tư: Phân/Thuốc - Nhật ký sử dụng vật tư
/// </summary>
[Table("material_usages")]
public class MaterialUsage
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("season_id")]
    [Required]
    public Guid SeasonId { get; set; } // Bón cho cây nào?

    [Column("usage_date")]
    public DateTime UsageDate { get; set; } = DateTime.UtcNow.Date;

    [Column("material_name")]
    [MaxLength(150)]
    public string? MaterialName { get; set; } // "NPK 16-16-8 Đầu Trâu"

    [Column("quantity")]
    [Precision(10, 2)]
    public decimal Quantity { get; set; } // 5.5

    [Column("unit")]
    [MaxLength(20)]
    public string? Unit { get; set; } // "Bao", "Lít", "Chai"

    [Column("unit_price")]
    [Precision(15, 2)]
    public decimal UnitPrice { get; set; } // Giá mua vào

    [Column("total_cost")]
    [Precision(15, 2)]
    public decimal TotalCost { get; set; } // Thành tiền

    [Column("note")]
    public string? Note { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(SeasonId))]
    public virtual CropSeason CropSeason { get; set; } = null!;

    [Column("material_id")]
    public Guid? MaterialId { get; set; }

    [ForeignKey(nameof(MaterialId))]
    public virtual Material? Material { get; set; }

    // Soft Delete
    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }
}
