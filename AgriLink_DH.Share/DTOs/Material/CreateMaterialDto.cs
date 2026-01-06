using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.Material;

public class CreateMaterialDto
{
    [Required(ErrorMessage = "Tên vật tư là bắt buộc")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Đơn vị tính là bắt buộc")]
    public string Unit { get; set; } = string.Empty;

    public decimal QuantityInStock { get; set; } = 0; // Số lượng tồn kho ban đầu

    public decimal CostPerUnit { get; set; } = 0;

    public string? Note { get; set; }
    public string? ImageUrl { get; set; }
}
