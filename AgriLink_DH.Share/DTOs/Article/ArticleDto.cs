using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Share.DTOs.Article;

/// <summary>
/// DTO cho Article Response đầy đủ
/// </summary>
public class ArticleDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public ArticleCategoryType CategoryCode { get; set; }
    
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAvatar { get; set; }
    public bool AuthorVerified { get; set; }
    public string? AuthorOrganization { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Content { get; set; }
    public string? ThumbnailUrl { get; set; }
    
    public List<string> Tags { get; set; } = new();
    public List<string> Hashtags { get; set; } = new();
    
    public int ReadTime { get; set; }
    public string? AudioUrl { get; set; }
    public int? AudioDuration { get; set; }
    public string? VideoUrl { get; set; }
    
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int ShareCount { get; set; }
    
    public ArticleStatus Status { get; set; }
    public bool IsFeatured { get; set; }
    public bool AllowComments { get; set; }
    
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
