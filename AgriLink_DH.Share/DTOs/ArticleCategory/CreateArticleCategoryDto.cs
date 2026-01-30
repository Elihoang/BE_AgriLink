using AgriLink_DH.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.ArticleCategory;

/// <summary>
/// DTO để tạo mới ArticleCategory
/// </summary>
public class CreateArticleCategoryDto
{
    [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
    [MaxLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã danh mục là bắt buộc")]
    public ArticleCategoryType Code { get; set; }

    [MaxLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Icon { get; set; }

    [MaxLength(20)]
    public string? Color { get; set; }

    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}
