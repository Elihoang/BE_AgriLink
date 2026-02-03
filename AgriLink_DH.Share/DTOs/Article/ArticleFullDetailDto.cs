using AgriLink_DH.Share.DTOs.ArticleComment;

namespace AgriLink_DH.Share.DTOs.Article;

/// <summary>
/// DTO cho article detail đầy đủ (article + comments + likes)
/// </summary>
public class ArticleFullDetailDto
{
    public ArticleDto Article { get; set; } = null!;
    public IEnumerable<ArticleCommentDto> Comments { get; set; } = new List<ArticleCommentDto>();
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool UserHasLiked { get; set; }
}
