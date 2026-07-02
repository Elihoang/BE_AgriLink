using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.ArticleLike;

namespace AgriLink_DH.Core.Validations;

public class ArticleLikeValidator
{
    private readonly IArticleRepository _articleRepository;
    private readonly IArticleLikeRepository _likeRepository;

    public ArticleLikeValidator(IArticleRepository articleRepository, IArticleLikeRepository likeRepository)
    {
        _articleRepository = articleRepository;
        _likeRepository = likeRepository;
    }

    public async Task ValidateLikeArticleAsync(LikeArticleDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        var article = await _articleRepository.GetByIdAsync(dto.ArticleId, cancellationToken);
        if (article == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bài viết với ID: {dto.ArticleId}");
        }

        var existingLike = await _likeRepository.GetByArticleAndUserAsync(dto.ArticleId, userId, cancellationToken);
        if (existingLike != null)
        {
            throw new InvalidOperationException("Bạn đã thích bài viết này rồi");
        }
    }

    public void ValidateUnlikeArticle(ArticleLike? like)
    {
        if (like == null)
        {
            throw new InvalidOperationException("Bạn chưa thích bài viết này");
        }
    }
}
