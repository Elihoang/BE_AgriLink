using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AgriLink_DH.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Lịch sử thanh toán lương qua MoMo
/// </summary>
[Table("salary_payments")]
public class SalaryPayment
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("worker_id")]
    [Required]
    public Guid WorkerId { get; set; }

    [Column("period_start")]
    public DateTime PeriodStart { get; set; }

    [Column("period_end")]
    public DateTime PeriodEnd { get; set; }

    [Column("gross_salary")]
    [Precision(15, 2)]
    public decimal GrossSalary { get; set; }

    [Column("total_advance")]
    [Precision(15, 2)]
    public decimal TotalAdvance { get; set; }

    [Column("net_salary")]
    [Precision(15, 2)]
    public decimal NetSalary { get; set; }

    [Column("momo_order_id")]
    [MaxLength(100)]
    public string? MomoOrderId { get; set; }

    [Column("momo_trans_id")]
    [MaxLength(100)]
    public string? MomoTransId { get; set; }

    [Column("momo_result_code")]
    public int? MomoResultCode { get; set; }

    [Column("status")]
    public SalaryPaymentStatus Status { get; set; } = SalaryPaymentStatus.Pending;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(WorkerId))]
    public virtual Worker Worker { get; set; } = null!;
}
