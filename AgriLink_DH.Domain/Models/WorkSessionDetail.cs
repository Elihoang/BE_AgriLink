using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Chi tiết Lương - Lines (Ghi nhận: Ai làm? Trả bao nhiêu?)
/// </summary>
[Table("work_session_details")]
public class WorkSessionDetail
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("session_id")]
    [Required]
    public Guid SessionId { get; set; }

    [Column("worker_id")]
    [Required]
    public Guid WorkerId { get; set; }

    [Column("payment_method")]
    [MaxLength(20)]
    public PaymentMethod PaymentMethod { get; set; } // 'DAILY' (Công nhật), 'PRODUCT' (Khoán SP)

    [Column("quantity")]
    [Precision(10, 2)]
    public decimal Quantity { get; set; } // 1 (ngày), 0.5 (buổi), 200 (kg), 50 (gốc)

    [Column("unit_price")]
    [Precision(15, 2)]
    public decimal UnitPrice { get; set; } // Cho phép sửa giá khác nhau với từng người

    [Column("total_amount")]
    [Precision(15, 2)]
    public decimal TotalAmount { get; set; } // = quantity * unit_price

    // Navigation Properties
    [ForeignKey(nameof(SessionId))]
    public virtual WorkSession WorkSession { get; set; } = null!;

    [ForeignKey(nameof(WorkerId))]
    public virtual Worker Worker { get; set; } = null!;
}
