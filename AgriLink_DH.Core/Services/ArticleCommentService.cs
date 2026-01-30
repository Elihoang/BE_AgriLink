using AgriLink_DH.Domain.Common;
using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
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

    public ArticleCommentService(
        IArticleCommentRepository commentRepository,
        IArticleRepository articleRepository,
        IUnitOfWork unitOfWork)
    {
        _commentRepository = commentRepository;
        _articleRepository = articleRepository;
        _unitOfWork = unitOfWork;
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
        // Validate Article exists and allows comments
        var article = await _articleRepository.GetByIdAsync(dto.ArticleId, cancellationToken);
        if (article == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bài viết với ID: {dto.ArticleId}");
        }

        if (!article.AllowComments)
        {
            throw new InvalidOperationException("Bài viết này không cho phép bình luận");
        }

        // If it's a reply, validate parent comment exists
        if (dto.ParentCommentId.HasValue)
        {
            var parentComment = await _commentRepository.GetByIdAsync(dto.ParentCommentId.Value, cancellationToken);
            if (parentComment == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy bình luận cha với ID: {dto.ParentCommentId}");
            }
        }

        var comment = new ArticleComment
        {
            Id = Guid.NewGuid(),
            ArticleId = dto.ArticleId,
            UserId = userId,
            ParentCommentId = dto.ParentCommentId,
            Content = dto.Content,
            Status = CommentStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepository.AddAsync(comment, cancellationToken);

        // Increment article comment count
        article.CommentCount++;
        _articleRepository.Update(article);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(comment);
    }

    public async Task<ArticleCommentDto> UpdateCommentAsync(Guid id, UpdateArticleCommentDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);
        if (comment == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bình luận với ID: {id}");
        }

        // Verify user owns the comment
        if (comment.UserId != userId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền sửa bình luận này");
        }

        comment.Content = dto.Content;
        comment.UpdatedAt = DateTime.UtcNow;

        _commentRepository.Update(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(comment);
    }

    public async Task<bool> DeleteCommentAsync(Guid id, Guid userId, bool isAdmin = false, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);
        if (comment == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bình luận với ID: {id}");
        }

        // Verify user owns the comment or is admin
        if (comment.UserId != userId && !isAdmin)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xóa bình luận này");
        }

        // Soft delete by changing status
        comment.Status = CommentStatus.Deleted;
        comment.UpdatedAt = DateTime.UtcNow;

        _commentRepository.Update(comment);

        // Decrement article comment count
        var article = await _articleRepository.GetByIdAsync(comment.ArticleId, cancellationToken);
        if (article != null && article.CommentCount > 0)
        {
            article.CommentCount--;
            _articleRepository.Update(article);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> HideCommentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);
        if (comment == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bình luận với ID: {id}");
        }

        comment.Status = CommentStatus.Hidden;
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
