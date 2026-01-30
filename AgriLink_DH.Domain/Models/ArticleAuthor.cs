using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Thông tin tác giả bài viết
/// Có thể là chuyên gia, viện nghiên cứu, hoặc biên tập viên
/// </summary>
[Table("article_authors")]
public class ArticleAuthor
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tên tác giả (ThS. Lê Văn Nam, Viện Nông Nghiệp,...)
    /// </summary>
    [Column("name")]
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Học vị/Chức danh (ThS, TS, Kỹ sư,...)
    /// </summary>
    [Column("title")]
    [MaxLength(50)]
    public string? Title { get; set; }

    /// <summary>
    /// Tổ chức/Cơ quan (Viện KHKT Nông Lâm Nghiệp Tây Nguyên,...)
    /// </summary>
    [Column("organization")]
    [MaxLength(200)]
    public string? Organization { get; set; }

    /// <summary>
    /// Email liên hệ
    /// </summary>
    [Column("email")]
    [MaxLength(100)]
    public string? Email { get; set; }

    /// <summary>
    /// Số điện thoại
    /// </summary>
    [Column("phone")]
    [MaxLength(20)]
    public string? Phone { get; set; }

    /// <summary>
    /// URL ảnh đại diện
    /// </summary>
    [Column("avatar_url")]
    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Tiểu sử/Giới thiệu ngắn
    /// </summary>
    [Column("bio")]
    [MaxLength(1000)]
    public string? Bio { get; set; }

    /// <summary>
    /// Trạng thái xác minh tác giả (verified badge)
    /// </summary>
    [Column("is_verified")]
    public bool IsVerified { get; set; } = false;

    /// <summary>
    /// Liên kết mạng xã hội (JSON)
    /// </summary>
    [Column("social_links")]
    [MaxLength(1000)]
    public string? SocialLinks { get; set; } // JSON: {"facebook":"url","linkedin":"url"}

    /// <summary>
    /// Chuyên môn (JSON array)
    /// </summary>
    [Column("specialties")]
    [MaxLength(500)]
    public string? Specialties { get; set; } // JSON: ["Cà phê","Hồ tiêu","Canh tác hữu cơ"]

    /// <summary>
    /// Kích hoạt
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
