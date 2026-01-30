using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Bài viết tri thức nông nghiệp
/// Chứa hướng dẫn kỹ thuật, kiến thức chuyên môn, tin tức thị trường,...
/// </summary>
[Table("articles")]
public class Article
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign Key đến ArticleCategory
    /// </summary>
    [Column("category_id")]
    [Required]
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Navigation property đến ArticleCategory
    /// </summary>
    [ForeignKey("CategoryId")]
    public virtual ArticleCategory? Category { get; set; }

    /// <summary>
    /// Foreign Key đến ArticleAuthor
    /// </summary>
    [Column("author_id")]
    [Required]
    public Guid AuthorId { get; set; }

    /// <summary>
    /// Navigation property đến ArticleAuthor
    /// </summary>
    [ForeignKey("AuthorId")]
    public virtual ArticleAuthor? Author { get; set; }

    /// <summary>
    /// Tiêu đề bài viết
    /// </summary>
    [Column("title")]
    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Slug URL-friendly (toi-uu-hoa-nang-suat-ca-phe-robusta-mua-kho-2024)
    /// </summary>
    [Column("slug")]
    [Required]
    [MaxLength(300)]
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả ngắn/Tóm tắt
    /// </summary>
    [Column("description")]
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Nội dung bài viết đầy đủ (HTML hoặc Markdown)
    /// </summary>
    [Column("content")]
    public string? Content { get; set; }

    /// <summary>
    /// URL ảnh thumbnail/cover
    /// </summary>
    [Column("thumbnail_url")]
    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Tags phân loại (JSON array)
    /// </summary>
    [Column("tags")]
    [MaxLength(500)]
    public string? Tags { get; set; } // JSON: ["Kỹ thuật canh tác","Cà phê Robusta"]

    /// <summary>
    /// Hashtags (JSON array)
    /// </summary>
    [Column("hashtags")]
    [MaxLength(500)]
    public string? Hashtags { get; set; } // JSON: ["#CaPheRobusta","#KyThuatCanhTac","#MuaKho2024"]

    /// <summary>
    /// Thời gian đọc ước tính (phút)
    /// </summary>
    [Column("read_time")]
    public int ReadTime { get; set; } = 5;

    /// <summary>
    /// URL file audio (nếu hỗ trợ nghe bài)
    /// </summary>
    [Column("audio_url")]
    [MaxLength(500)]
    public string? AudioUrl { get; set; }

    /// <summary>
    /// Thời lượng audio (giây)
    /// </summary>
    [Column("audio_duration")]
    public int? AudioDuration { get; set; }

    /// <summary>
    /// URL video liên quan (nếu có)
    /// </summary>
    [Column("video_url")]
    [MaxLength(500)]
    public string? VideoUrl { get; set; }

    /// <summary>
    /// Số lượt xem
    /// </summary>
    [Column("view_count")]
    public int ViewCount { get; set; } = 0;

    /// <summary>
    /// Số lượt thích
    /// </summary>
    [Column("like_count")]
    public int LikeCount { get; set; } = 0;

    /// <summary>
    /// Số lượt bình luận
    /// </summary>
    [Column("comment_count")]
    public int CommentCount { get; set; } = 0;

    /// <summary>
    /// Số lượt chia sẻ
    /// </summary>
    [Column("share_count")]
    public int ShareCount { get; set; } = 0;

    /// <summary>
    /// Trạng thái xuất bản
    /// </summary>
    [Column("status")]
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;

    /// <summary>
    /// Bài viết nổi bật
    /// </summary>
    [Column("is_featured")]
    public bool IsFeatured { get; set; } = false;

    /// <summary>
    /// Cho phép bình luận
    /// </summary>
    [Column("allow_comments")]
    public bool AllowComments { get; set; } = true;

    /// <summary>
    /// Ngày xuất bản
    /// </summary>
    [Column("published_at")]
    public DateTime? PublishedAt { get; set; }

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

    /// <summary>
    /// Người tạo (User ID)
    /// </summary>
    [Column("created_by")]
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Người cập nhật cuối (User ID)
    /// </summary>
    [Column("updated_by")]
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Metadata SEO (JSON)
    /// </summary>
    [Column("seo_metadata")]
    [MaxLength(2000)]
    public string? SeoMetadata { get; set; } // JSON: {"title":"","description":"","keywords":""}

    // Navigation Properties
    public virtual ICollection<ArticleComment> Comments { get; set; } = new List<ArticleComment>();
    public virtual ICollection<ArticleLike> Likes { get; set; } = new List<ArticleLike>();
}
