using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Đơn giá Công việc Mẫu - Giúp nhập liệu nhanh
/// </summary>
[Table("task_types")]
public class TaskType
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("farm_id")]
    [Required]
    public Guid FarmId { get; set; }

    [Column("name")]
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // "Làm cành", "Hái khoán", "Bón phân"

    [Column("default_unit")]
    [MaxLength(20)]
    public string? DefaultUnit { get; set; } // 'CONG' (Ngày), 'KG', 'GOC' (Gốc)

    [Column("default_price")]
    [Precision(15, 2)]
    public decimal? DefaultPrice { get; set; } // Giá gợi ý. VD: 350000 hoặc 1200

    // Navigation Properties
    [ForeignKey(nameof(FarmId))]
    public virtual Farm Farm { get; set; } = null!;
}
