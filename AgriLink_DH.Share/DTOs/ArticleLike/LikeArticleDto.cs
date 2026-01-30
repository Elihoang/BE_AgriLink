using System.ComponentModel.DataAnnotations;

namespace AgriLink_DH.Share.DTOs.ArticleLike;

public class LikeArticleDto
{
    [Required]
    public Guid ArticleId { get; set; }
}
