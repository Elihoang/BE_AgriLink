using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Domain.Models;

[Table("harvest_sessions")]
public class HarvestSession
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("season_id")]
    [Required]
    public Guid SeasonId { get; set; } // Thu hoạch của vụ nào

    [Column("harvest_date")]
    public DateTime HarvestDate { get; set; } = DateTime.UtcNow.Date;

    [Column("total_bags")]
    public int TotalBags { get; set; } = 0; // Tổng số bao (Auto trigger)

    [Column("total_weight")]
    [Precision(10, 2)]
    public decimal TotalWeight { get; set; } = 0; // Tổng kg (Auto trigger)

    [Column("storage_location")]
    [MaxLength(50)]
    public string? StorageLocation { get; set; } // "KHO_NHA", "DAI_LY_A" (Chỉ ghi chú)

    // Navigation Properties
    [ForeignKey(nameof(SeasonId))]
    public virtual CropSeason CropSeason { get; set; } = null!;

    public virtual ICollection<HarvestBagDetail> HarvestBagDetails { get; set; } = new List<HarvestBagDetail>();

    // Soft Delete
    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }
}
