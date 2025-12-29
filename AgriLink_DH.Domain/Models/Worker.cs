using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AgriLink_DH.Domain.Common;

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

    [Column("farm_id")]
    [Required]
    public Guid FarmId { get; set; }

    [Column("full_name")]
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty; // "Chú Bảy", "Tèo"

    [Column("phone")]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [Column("worker_type")]
    [MaxLength(20)]
    public WorkerType WorkerType { get; set; } = WorkerType.SEASONAL;

    [Column("is_active")]
    public bool IsActive { get; set; } = true; // False nếu đã nghỉ việc

    // Navigation Properties
    [ForeignKey(nameof(FarmId))]
    public virtual Farm Farm { get; set; } = null!;

    public virtual ICollection<WorkSessionDetail> WorkSessionDetails { get; set; } = new List<WorkSessionDetail>();
    public virtual ICollection<WorkerAdvance> WorkerAdvances { get; set; } = new List<WorkerAdvance>();
}
