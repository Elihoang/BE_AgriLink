using AgriLink_DH.Domain.Common;
using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Core.Validations;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.ArticleComment;

namespace AgriLink_DH.Core.Services;

/// <summary>
/// Service xử lý business logic cho ArticleComment
/// </summary>
public class ArticleCommentService
{
    private readonly IArticleCommentRepository _commentRepository;
    private readonly IArticleRepository _articleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ArticleCommentValidator _validator;

    public ArticleCommentService(
        IArticleCommentRepository commentRepository,
        IArticleRepository articleRepository,
        IUnitOfWork unitOfWork,
        ArticleCommentValidator validator)
    {
        _commentRepository = commentRepository;
        _articleRepository = articleRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<IEnumerable<ArticleCommentDto>> GetCommentsByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default)
    {
        var comments = await _commentRepository.GetByArticleIdAsync(articleId, cancellationToken);
        return comments.Select(MapToDto);
    }

    public async Task<IEnumerable<ArticleCommentDto>> GetRepliesByCommentIdAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        var replies = await _commentRepository.GetRepliesByCommentIdAsync(commentId, cancellationToken);
        return replies.Select(MapToDto);
    }

    public async Task<IEnumerable<ArticleCommentDto>> GetCommentsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var comments = await _commentRepository.GetByUserIdAsync(userId, cancellationToken);
        return comments.Select(MapToDto);
    }

    public async Task<ArticleCommentDto?> GetCommentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);
        return comment != null ? MapToDto(comment) : null;
    }

    public async Task<ArticleCommentDto> CreateCommentAsync(CreateArticleCommentDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateCreateCommentAsync(dto, cancellationToken);
        var article = (await _articleRepository.GetByIdAsync(dto.ArticleId, cancellationToken))!;

        var comment = new ArticleComment
        {
            ArticleId = dto.ArticleId,
            UserId = userId,
            ParentCommentId = dto.ParentCommentId,
            Content = dto.Content,
            Status = CommentStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepository.AddAsync(comment, cancellationToken);

        // Increment article comment count
        article.IncrementCommentCount();
        _articleRepository.Update(article);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(comment);
    }

    public async Task<ArticleCommentDto> UpdateCommentAsync(Guid id, UpdateArticleCommentDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);
        _validator.ValidateUpdateComment(comment, id, userId);

        comment!.Content = dto.Content;
        comment.UpdatedAt = DateTime.UtcNow;

        _commentRepository.Update(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(comment);
    }

    public async Task<bool> DeleteCommentAsync(Guid id, Guid userId, bool isAdmin = false, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);
        _validator.ValidateDeleteComment(comment, id, userId, isAdmin);

        // Soft delete by changing status
        comment!.Status = CommentStatus.Deleted;
        comment.UpdatedAt = DateTime.UtcNow;

        _commentRepository.Update(comment);

        // Decrement article comment count
        var article = await _articleRepository.GetByIdAsync(comment.ArticleId, cancellationToken);
        if (article != null)
        {
            article.DecrementCommentCount();
            _articleRepository.Update(article);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> HideCommentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);
        _validator.ValidateHideComment(comment, id);

        comment!.Status = CommentStatus.Hidden;
        comment.UpdatedAt = DateTime.UtcNow;

        _commentRepository.Update(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static ArticleCommentDto MapToDto(ArticleComment comment)
    {
        return new ArticleCommentDto
        {
            Id = comment.Id,
            ArticleId = comment.ArticleId,
            UserId = comment.UserId,
            UserName = comment.User?.FullName ?? "Unknown",
            UserAvatar = null, // TODO: Add AvatarUrl to User model if needed
            ParentCommentId = comment.ParentCommentId,
            Content = comment.Content,
            LikeCount = comment.LikeCount,
            Status = comment.Status,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }
}
