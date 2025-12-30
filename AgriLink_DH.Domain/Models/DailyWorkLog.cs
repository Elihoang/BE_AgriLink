using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Nhật ký công việc tại Vườn - Header (Ghi nhận: Hôm nay tại Vườn A có hoạt động gì?)
/// </summary>
[Table("daily_work_logs")]
public class DailyWorkLog
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("season_id")]
    [Required]
    public Guid SeasonId { get; set; } // Gắn việc vào Vụ/Vườn cụ thể

    [Column("work_date")]
    public DateTime WorkDate { get; set; } = DateTime.UtcNow.Date;

    [Column("task_type_id")]
    public Guid? TaskTypeId { get; set; } // Link tới loại công việc

    [Column("note")]
    public string? Note { get; set; }

    [Column("total_cost")]
    [Precision(15, 2)]
    public decimal TotalCost { get; set; } = 0; // Tổng chi phí trong ngày cho đầu việc này (Tự động cộng dồn)

    // Navigation Properties
    [ForeignKey(nameof(SeasonId))]
    public virtual CropSeason CropSeason { get; set; } = null!;

    [ForeignKey(nameof(TaskTypeId))]
    public virtual TaskType? TaskType { get; set; }

    public virtual ICollection<WorkAssignment> WorkAssignments { get; set; } = new List<WorkAssignment>();

    // Soft Delete
    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }
}
