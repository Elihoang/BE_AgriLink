namespace AgriLink_DH.Share.DTOs.ArticleLike;

public class ArticleLikeDto
{
    public Guid Id { get; set; }
    public Guid ArticleId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsLiked { get; set; }
}
