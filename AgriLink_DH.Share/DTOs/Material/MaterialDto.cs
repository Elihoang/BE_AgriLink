using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Share.DTOs.Material;

public class MaterialDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal QuantityInStock { get; set; }
    public decimal CostPerUnit { get; set; }
    public string? Note { get; set; }
    public string? ImageUrl { get; set; }
    public MaterialType MaterialType { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

