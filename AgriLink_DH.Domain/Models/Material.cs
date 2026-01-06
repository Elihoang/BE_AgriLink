using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgriLink_DH.Domain.Models;

public class Material
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid OwnerUserId { get; set; }

    [ForeignKey(nameof(OwnerUserId))]
    public User? Owner { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty; // kg, lít, bao, chai...

    [Column(TypeName = "decimal(18,2)")]
    public decimal QuantityInStock { get; set; } = 0; // Số lượng tồn kho

    [Column(TypeName = "decimal(18,2)")]
    public decimal CostPerUnit { get; set; } = 0; // Đơn giá ước tính (để tính chi phí khi xuất kho)

    [MaxLength(500)]
    public string? Note { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
