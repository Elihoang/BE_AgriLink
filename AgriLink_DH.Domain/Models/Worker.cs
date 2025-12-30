using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AgriLink_DH.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Danh sách Nhân công - Quản lý hồ sơ người làm
/// </summary>
[Table("workers")]
public class Worker
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("full_name")]
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty; // "Chú Bảy", "Tèo"

    [Column("phone")]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [Column("worker_type")]
    public WorkerType WorkerType { get; set; } = WorkerType.Seasonal;

    [Column("default_daily_wage")]
    [Precision(15, 2)]
    public decimal? DefaultDailyWage { get; set; } // Lương ngày mặc định

    [Column("is_active")]
    public bool IsActive { get; set; } = true; // False nếu đã nghỉ việc

    // Navigation Properties
    // No Farm Navigation property needed here as it belongs to User

    public virtual ICollection<WorkAssignment> WorkAssignments { get; set; } = new List<WorkAssignment>();
    public virtual ICollection<WorkerAdvance> WorkerAdvances { get; set; } = new List<WorkerAdvance>();
}
