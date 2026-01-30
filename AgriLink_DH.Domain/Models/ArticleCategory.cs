using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Danh mục bài viết tri thức nông nghiệp
/// Phân loại: Kỹ thuật canh tác, Phòng bệnh, Giống cây trồng, Thị trường,...
/// </summary>
[Table("article_categories")]
public class ArticleCategory
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tên danh mục (Kỹ thuật canh tác, Phòng bệnh, Thị trường,...)
    /// </summary>
    [Column("name")]
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mã danh mục (enum type-safe)
    /// </summary>
    [Column("code")]
    public ArticleCategoryType Code { get; set; }

    /// <summary>
    /// Mô tả danh mục
    /// </summary>
    [Column("description")]
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Icon/Emoji đại diện
    /// </summary>
    [Column("icon")]
    [MaxLength(50)]
    public string? Icon { get; set; }

    /// <summary>
    /// Màu sắc nhận diện (hex color)
    /// </summary>
    [Column("color")]
    [MaxLength(20)]
    public string? Color { get; set; }

    /// <summary>
    /// Thứ tự hiển thị
    /// </summary>
    [Column("display_order")]
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Kích hoạt sử dụng
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời gian cập nhật
    /// </summary>
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}
