using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Niên vụ - Trái tim của hệ thống. Tách bạch chi phí Xen canh
/// </summary>
[Table("crop_seasons")]
public class CropSeason
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("farm_id")]
    [Required]
    public Guid FarmId { get; set; }

    [Column("product_id")]
    [Required]
    public Guid ProductId { get; set; } // QUAN TRỌNG: Vụ này của cây gì?

    [Column("name")]
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // "Vụ Cà 2025", "Vụ Tiêu 2025"

    [Column("start_date")]
    public DateTime? StartDate { get; set; }

    [Column("end_date")]
    public DateTime? EndDate { get; set; }

    [Column("status")]
    [MaxLength(20)]
    public SeasonStatus Status { get; set; } = SeasonStatus.Active;

    [Column("note")]
    public string? Note { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(FarmId))]
    public virtual Farm Farm { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<DailyWorkLog> DailyWorkLogs { get; set; } = new List<DailyWorkLog>();
    public virtual ICollection<WorkerAdvance> WorkerAdvances { get; set; } = new List<WorkerAdvance>();
    public virtual ICollection<MaterialUsage> MaterialUsages { get; set; } = new List<MaterialUsage>();
    public virtual ICollection<HarvestSession> HarvestSessions { get; set; } = new List<HarvestSession>();
    public virtual ICollection<FarmSale> FarmSales { get; set; } = new List<FarmSale>();

    // Soft Delete
    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }
}
