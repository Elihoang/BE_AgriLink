using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Share.DTOs.ArticleComment;

public class ArticleCommentDto
{
    public Guid Id { get; set; }
    public Guid ArticleId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int LikeCount { get; set; }
    public CommentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
