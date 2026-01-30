using AgriLink_DH.Domain.Common;

namespace AgriLink_DH.Share.DTOs.ArticleCategory;

/// <summary>
/// DTO cho ArticleCategory Response
/// </summary>
public class ArticleCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ArticleCategoryType Code { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
