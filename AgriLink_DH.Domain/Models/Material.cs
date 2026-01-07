using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Domain.Models;

[Table("materials")]
public class Material
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("owner_user_id")]
    [Required]
    public Guid OwnerUserId { get; set; }

    [ForeignKey(nameof(OwnerUserId))]
    public User? Owner { get; set; }

    [Column("name")]
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Column("unit")]
    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty; // kg, lít, bao, chai...

    [Column("quantity_in_stock", TypeName = "decimal(18,2)")]
    public decimal QuantityInStock { get; set; } = 0; // Số lượng tồn kho

    [Column("cost_per_unit", TypeName = "decimal(18,2)")]
    public decimal CostPerUnit { get; set; } = 0; // Đơn giá ước tính (để tính chi phí khi xuất kho)

    [Column("note")]
    [MaxLength(500)]
    public string? Note { get; set; }

    [Column("image_url")]
    public string? ImageUrl { get; set; }

    [Column("material_type")]
    public MaterialType MaterialType { get; set; } = MaterialType.Other;

    [Column("expiry_date")]
    public DateTime? ExpiryDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}



