using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.Article;

/// <summary>
/// DTO để tạo mới Article
/// </summary>
public class CreateArticleDto
{
    [Required(ErrorMessage = "CategoryId là bắt buộc")]
    public Guid CategoryId { get; set; }

    [Required(ErrorMessage = "AuthorId là bắt buộc")]
    public Guid AuthorId { get; set; }

    [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
    [MaxLength(300, ErrorMessage = "Tiêu đề không được vượt quá 300 ký tự")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
    public string? Description { get; set; }

    public string? Content { get; set; }

    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }

    public List<string>? Tags { get; set; }
    public List<string>? Hashtags { get; set; }

    [Range(1, 300, ErrorMessage = "Thời gian đọc phải từ 1-300 phút")]
    public int ReadTime { get; set; } = 5;

    [MaxLength(500)]
    public string? AudioUrl { get; set; }

    [Range(0, int.MaxValue)]
    public int? AudioDuration { get; set; }

    [MaxLength(500)]
    public string? VideoUrl { get; set; }

    public bool IsFeatured { get; set; } = false;
    public bool AllowComments { get; set; } = true;

    // Nếu muốn publish ngay khi tạo
    public bool PublishImmediately { get; set; } = false;
}
