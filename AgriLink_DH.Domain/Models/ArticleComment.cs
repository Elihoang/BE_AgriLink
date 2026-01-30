using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Bình luận của người dùng trên bài viết
/// </summary>
[Table("article_comments")]
public class ArticleComment
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign Key đến Article
    /// </summary>
    [Column("article_id")]
    [Required]
    public Guid ArticleId { get; set; }

    /// <summary>
    /// Navigation property đến Article
    /// </summary>
    [ForeignKey("ArticleId")]
    public virtual Article? Article { get; set; }

    /// <summary>
    /// Foreign Key đến User (người bình luận)
    /// </summary>
    [Column("user_id")]
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property đến User
    /// </summary>
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    /// <summary>
    /// ID bình luận cha (nếu là reply)
    /// </summary>
    [Column("parent_comment_id")]
    public Guid? ParentCommentId { get; set; }

    /// <summary>
    /// Navigation property đến bình luận cha
    /// </summary>
    [ForeignKey("ParentCommentId")]
    public virtual ArticleComment? ParentComment { get; set; }

    /// <summary>
    /// Nội dung bình luận
    /// </summary>
    [Column("content")]
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Số lượt thích bình luận này
    /// </summary>
    [Column("like_count")]
    public int LikeCount { get; set; } = 0;

    /// <summary>
    /// Trạng thái bình luận
    /// </summary>
    [Column("status")]
    public CommentStatus Status { get; set; } = CommentStatus.Active;

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
    public virtual ICollection<ArticleComment> Replies { get; set; } = new List<ArticleComment>();
}
