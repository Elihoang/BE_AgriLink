using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Danh mục Cây trồng Hệ thống 
/// </summary>
[Table("products")]
public class Product
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("name")]
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // "Cà phê Robusta", "Hồ Tiêu"

    [Column("unit")]
    [MaxLength(20)]
    public string Unit { get; set; } = "kg"; // Đơn vị tính chuẩn

    [Column("code")]
    [MaxLength(20)]
    public string? Code { get; set; } // "CF_ROBUSTA", "PEPPER" (Dùng để map logic code)

    [Column("image_url")]
    [MaxLength(500)]
    public string? ImageUrl { get; set; } // URL hình ảnh sản phẩm

    // Navigation Properties
    public virtual ICollection<CropSeason> CropSeasons { get; set; } = new List<CropSeason>();
}
