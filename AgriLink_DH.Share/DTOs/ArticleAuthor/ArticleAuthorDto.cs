namespace AgriLink_DH.Share.DTOs.ArticleAuthor;

/// <summary>
/// DTO cho ArticleAuthor Response
/// </summary>
public class ArticleAuthorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Organization { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public bool IsVerified { get; set; }
    public List<string> Specialties { get; set; } = new();
    public Dictionary<string, string> SocialLinks { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
