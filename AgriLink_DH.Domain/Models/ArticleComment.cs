using AgriLink_DH.Domain.Common;

using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Bình luận của người dùng trên bài viết
/// </summary>
public class ArticleComment : BaseEntity
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
    /// Foreign Key đến User (người bình luận)
    /// </summary>
            public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property đến User
    /// </summary>
        public virtual User? User { get; set; }

    /// <summary>
    /// ID bình luận cha (nếu là reply)
    /// </summary>
        public Guid? ParentCommentId { get; set; }

    /// <summary>
    /// Navigation property đến bình luận cha
    /// </summary>
        public virtual ArticleComment? ParentComment { get; set; }

    /// <summary>
    /// Nội dung bình luận
    /// </summary>
                public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Số lượt thích bình luận này
    /// </summary>
        public int LikeCount { get; set; } = 0;

    /// <summary>
    /// Trạng thái bình luận
    /// </summary>
        public CommentStatus Status { get; set; } = CommentStatus.Active;

    /// <summary>
    /// Thời gian tạo
    /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời gian cập nhật
    /// </summary>
        public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public virtual ICollection<ArticleComment> Replies { get; set; } = new List<ArticleComment>();
}
