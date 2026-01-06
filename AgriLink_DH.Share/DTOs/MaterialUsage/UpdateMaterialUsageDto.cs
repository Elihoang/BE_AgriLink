using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.MaterialUsage;

public class UpdateMaterialUsageDto
{
    public DateTime UsageDate { get; set; }
    
    public Guid? MaterialId { get; set; }

    [MaxLength(150)]
    public string? MaterialName { get; set; }

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
