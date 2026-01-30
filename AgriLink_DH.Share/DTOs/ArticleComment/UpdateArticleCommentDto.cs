using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.ArticleComment;

public class UpdateArticleCommentDto
{
    [Required(ErrorMessage = "Nội dung bình luận là bắt buộc")]
    [MaxLength(2000, ErrorMessage = "Bình luận không được vượt quá 2000 ký tự")]
    public string Content { get; set; } = string.Empty;
}
