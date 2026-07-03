using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.ArticleComment;

namespace AgriLink_DH.Core.Validations;

public class ArticleCommentValidator
{
    private readonly IArticleRepository _articleRepository;
    private readonly IArticleCommentRepository _commentRepository;

    public ArticleCommentValidator(IArticleRepository articleRepository, IArticleCommentRepository commentRepository)
    {
        _articleRepository = articleRepository;
        _commentRepository = commentRepository;
    }

    public async Task ValidateCreateCommentAsync(CreateArticleCommentDto dto, CancellationToken cancellationToken = default)
    {
        var article = await _articleRepository.GetByIdAsync(dto.ArticleId, cancellationToken);
        if (article == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bài viết với ID: {dto.ArticleId}");
        }

        if (!article.AllowComments)
        {
            throw new InvalidOperationException("Bài viết này không cho phép bình luận");
        }

        if (dto.ParentCommentId.HasValue)
        {
            var parentComment = await _commentRepository.GetByIdAsync(dto.ParentCommentId.Value, cancellationToken);
            if (parentComment == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy bình luận cha với ID: {dto.ParentCommentId}");
            }
        }
    }

    public void ValidateUpdateComment(ArticleComment? comment, Guid id, Guid userId)
    {
        if (comment == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bình luận với ID: {id}");
        }

        if (comment.UserId != userId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền sửa bình luận này");
        }
    }

    public void ValidateDeleteComment(ArticleComment? comment, Guid id, Guid userId, bool isAdmin)
    {
        if (comment == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bình luận với ID: {id}");
        }

        if (comment.UserId != userId && !isAdmin)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xóa bình luận này");
        }
    }

    public void ValidateHideComment(ArticleComment? comment, Guid id)
    {
        if (comment == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bình luận với ID: {id}");
        }
    }
}
