using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Lượt thích bài viết của người dùng
/// </summary>
public class ArticleLike : BaseEntity
{

    /// <summary>
    /// Foreign Key đến Article
    /// </summary>
            public Guid ArticleId { get; set; }

    /// <summary>
    /// Navigation property đến Article
    /// </summary>
        public virtual Article? Article { get; set; }

    /// <summary>
    /// Foreign Key đến User (người thích)
    /// </summary>
            public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property đến User
    /// </summary>
        public virtual User? User { get; set; }

    /// <summary>
    /// Thời gian thích
    /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
