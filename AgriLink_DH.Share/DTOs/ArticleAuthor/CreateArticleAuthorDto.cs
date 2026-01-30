using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.ArticleAuthor;

/// <summary>
/// DTO để tạo mới ArticleAuthor
/// </summary>
public class CreateArticleAuthorDto
{
    [Required(ErrorMessage = "Tên tác giả là bắt buộc")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Title { get; set; }

    [MaxLength(200)]
    public string? Organization { get; set; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [MaxLength(100)]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    [MaxLength(1000)]
    public string? Bio { get; set; }

    public bool IsVerified { get; set; } = false;
    public List<string>? Specialties { get; set; }
    public Dictionary<string, string>? SocialLinks { get; set; }
    public bool IsActive { get; set; } = true;
}
