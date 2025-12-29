using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Nhật ký Làm việc - Header (Ghi nhận sự kiện: Hôm nay làm gì?)
/// </summary>
[Table("work_sessions")]
public class WorkSession
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("season_id")]
    [Required]
    public Guid SeasonId { get; set; } // Chi phí tính vào vụ nào

    [Column("work_date")]
    public DateTime WorkDate { get; set; } = DateTime.UtcNow.Date;

    [Column("task_name")]
    [MaxLength(100)]
    public string? TaskName { get; set; } // Snapshot tên từ task_types: "Làm cành đợt 1"

    [Column("total_cost")]
    [Precision(15, 2)]
    public decimal TotalCost { get; set; } = 0; // Tổng tiền chi (Auto Sum từ chi tiết)

    [Column("note")]
    public string? Note { get; set; } // "Làm khu vực suối, trời nắng gắt"

    // Navigation Properties
    [ForeignKey(nameof(SeasonId))]
    public virtual CropSeason CropSeason { get; set; } = null!;

    public virtual ICollection<WorkSessionDetail> WorkSessionDetails { get; set; } = new List<WorkSessionDetail>();
}
