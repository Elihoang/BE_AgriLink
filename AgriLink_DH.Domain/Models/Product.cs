using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Danh mục Cây trồng Hệ thống (Dữ liệu tĩnh, Admin tạo sẵn)
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

    // Navigation Properties
    public virtual ICollection<CropSeason> CropSeasons { get; set; } = new List<CropSeason>();
}
