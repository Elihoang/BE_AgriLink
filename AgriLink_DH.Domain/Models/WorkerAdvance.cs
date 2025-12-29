using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Sổ Ứng Lương - Quản lý công nợ với thợ
/// </summary>
[Table("worker_advances")]
public class WorkerAdvance
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("worker_id")]
    [Required]
    public Guid WorkerId { get; set; }

    [Column("season_id")]
    [Required]
    public Guid SeasonId { get; set; } // Hạch toán vào vụ hiện tại

    [Column("amount")]
    [Required]
    [Precision(15, 2)]
    public decimal Amount { get; set; } // Số tiền ứng: 500,000

    [Column("advance_date")]
    public DateTime AdvanceDate { get; set; } = DateTime.UtcNow.Date;

    [Column("is_deducted")]
    public bool IsDeducted { get; set; } = false; // True: Đã trừ vào lương, False: Chưa trừ

    [Column("note")]
    public string? Note { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(WorkerId))]
    public virtual Worker Worker { get; set; } = null!;

    [ForeignKey(nameof(SeasonId))]
    public virtual CropSeason CropSeason { get; set; } = null!;
}
