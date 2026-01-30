using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Lượt thích bài viết của người dùng
/// </summary>
[Table("article_likes")]
public class ArticleLike
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
    /// Foreign Key đến User (người thích)
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
    /// Thời gian thích
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
