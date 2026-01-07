using System.ComponentModel.DataAnnotations;
using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Share.DTOs.Material;

public class UpdateMaterialDto
{
    [Required(ErrorMessage = "Tên vật tư là bắt buộc")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Đơn vị tính là bắt buộc")]
    public string Unit { get; set; } = string.Empty;

    public decimal QuantityInStock { get; set; }

    public decimal CostPerUnit { get; set; }

    public string? Note { get; set; }
    public string? ImageUrl { get; set; }
    
    public MaterialType MaterialType { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

