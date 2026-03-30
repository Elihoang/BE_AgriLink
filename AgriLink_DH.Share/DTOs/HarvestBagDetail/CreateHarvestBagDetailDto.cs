using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.HarvestBagDetail;

public class CreateHarvestBagDetailDto
{
    [Required]
    public Guid SessionId { get; set; }

    [Range(1, int.MaxValue)]
    public int BagIndex { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal GrossWeight { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Deduction { get; set; } = 0.5m;

    public bool IsAutoWeighed { get; set; } = false;
    
    public string? ScaleDeviceId { get; set; }
}
