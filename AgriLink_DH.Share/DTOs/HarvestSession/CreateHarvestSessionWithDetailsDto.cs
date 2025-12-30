using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.HarvestSession;

/// <summary>
/// DTO để tạo Session + Details cùng lúc
/// </summary>
public class CreateHarvestSessionWithDetailsDto
{
    [Required]
    public Guid SeasonId { get; set; }

    public DateTime HarvestDate { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? StorageLocation { get; set; }

    /// <summary>
    /// Danh sách bao (nếu có)
    /// </summary>
    public List<BagDetailInput>? Bags { get; set; }
}

/// <summary>
/// Thông tin 1 bao
/// </summary>
public class BagDetailInput
{
    [Range(1, int.MaxValue)]
    public int BagIndex { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal GrossWeight { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Deduction { get; set; } = 0.5m;
}
