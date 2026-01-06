using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.MaterialUsage;

public class CreateMaterialUsageDto
{
    [Required]
    public Guid SeasonId { get; set; }

    public DateTime UsageDate { get; set; } = DateTime.UtcNow;

    public Guid? MaterialId { get; set; } // Link to Inventory
    
    [MaxLength(150)]
    public string? MaterialName { get; set; } // Snapshot or Free text

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Quantity { get; set; }

    [MaxLength(20)]
    public string? Unit { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    public string? Note { get; set; }
}
