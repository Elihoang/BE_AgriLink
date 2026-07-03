using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Core.Validations;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.ArticleLike;

namespace AgriLink_DH.Core.Services;

/// <summary>
/// Service xử lý business logic cho ArticleLike
/// </summary>
public class ArticleLikeService
{
    private readonly IArticleLikeRepository _likeRepository;
    private readonly IArticleRepository _articleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ArticleLikeValidator _validator;

    public ArticleLikeService(
        IArticleLikeRepository likeRepository,
        IArticleRepository articleRepository,
        IUnitOfWork unitOfWork,
        ArticleLikeValidator validator)
    {
        _likeRepository = likeRepository;
        _articleRepository = articleRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<bool> HasUserLikedArticleAsync(Guid articleId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _likeRepository.HasUserLikedArticleAsync(articleId, userId, cancellationToken);
    }

    public async Task<int> GetArticleLikeCountAsync(Guid articleId, CancellationToken cancellationToken = default)
    {
        return await _likeRepository.GetLikeCountByArticleIdAsync(articleId, cancellationToken);
    }

    public async Task<IEnumerable<ArticleLikeDto>> GetUserLikedArticlesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var likes = await _likeRepository.GetByUserIdAsync(userId, cancellationToken);
        return likes.Select(MapToDto);
    }

    public async Task<ArticleLikeDto> LikeArticleAsync(LikeArticleDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateLikeArticleAsync(dto, userId, cancellationToken);
        var article = (await _articleRepository.GetByIdAsync(dto.ArticleId, cancellationToken))!;

        var like = new ArticleLike
        {
            ArticleId = dto.ArticleId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _likeRepository.AddAsync(like, cancellationToken);

        // Increment article like count
        article.IncrementLikeCount();
        _articleRepository.Update(article);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(like);
    }

    public async Task<bool> UnlikeArticleAsync(Guid articleId, Guid userId, CancellationToken cancellationToken = default)
    {
        var like = await _likeRepository.GetByArticleAndUserAsync(articleId, userId, cancellationToken);
        _validator.ValidateUnlikeArticle(like);

        await _likeRepository.RemoveByIdAsync(like!.Id, cancellationToken);

        // Decrement article like count
        var article = await _articleRepository.GetByIdAsync(articleId, cancellationToken);
        if (article != null)
        {
            article.DecrementLikeCount();
            _articleRepository.Update(article);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<ArticleLikeDto> ToggleLikeAsync(Guid articleId, Guid userId, CancellationToken cancellationToken = default)
    {
        var hasLiked = await _likeRepository.HasUserLikedArticleAsync(articleId, userId, cancellationToken);
        
        if (hasLiked)
        {
            await UnlikeArticleAsync(articleId, userId, cancellationToken);
            return new ArticleLikeDto { ArticleId = articleId, UserId = userId, IsLiked = false };
        }
        else
        {
            var result = await LikeArticleAsync(new LikeArticleDto { ArticleId = articleId }, userId, cancellationToken);
            result.IsLiked = true;
            return result;
        }
    }

    private static ArticleLikeDto MapToDto(ArticleLike like)
    {
        return new ArticleLikeDto
        {
            Id = like.Id,
            ArticleId = like.ArticleId,
            UserId = like.UserId,
            CreatedAt = like.CreatedAt,
            IsLiked = true
        };
    }
}
