using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Bảng Chấm Công Chi Tiết - Lines (Ông Tèo hôm nay làm ở đây bao lâu, bao nhiêu tiền?)
/// </summary>
[Table("work_assignments")]
public class WorkAssignment
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("log_id")]
    [Required]
    public Guid LogId { get; set; } // Link tới Nhật ký ở trên

    [Column("worker_id")]
    [Required]
    public Guid WorkerId { get; set; } // Link tới ông Tèo

    [Column("payment_method")]
    [MaxLength(20)]
    public PaymentMethod PaymentMethod { get; set; } // 'DAILY', 'PRODUCT'

    [Column("quantity")]
    [Precision(10, 2)]
    public decimal Quantity { get; set; } // 0.5 (công), 200 (kg)

    [Column("unit_price")]
    [Precision(15, 2)]
    public decimal UnitPrice { get; set; } // Đơn giá (VD: 250k/công)

    [Column("total_amount")]
    [Precision(15, 2)]
    public decimal TotalAmount { get; set; } // Thành tiền (= quantity * unit_price)

    [Column("note")]
    public string? Note { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(LogId))]
    public virtual DailyWorkLog DailyWorkLog { get; set; } = null!;

    [ForeignKey(nameof(WorkerId))]
    public virtual Worker Worker { get; set; } = null!;
}
