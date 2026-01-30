namespace AgriLink_DH.Share.DTOs.Article;

/// <summary>
/// DTO cho Article List Item (simplified) - dùng cho danh sách
/// </summary>
public class ArticleListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    
    public string CategoryName { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAvatar { get; set; }
    public bool AuthorVerified { get; set; }
    
    public List<string> Tags { get; set; } = new();
    public int ReadTime { get; set; }
    
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    
    public DateTime? PublishedAt { get; set; }
    public string PublishedTimeAgo { get; set; } = string.Empty; // "một ngày trước"
}
