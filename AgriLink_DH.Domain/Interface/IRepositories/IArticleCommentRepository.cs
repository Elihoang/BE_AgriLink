using AgriLink_DH.Domain.Common;
using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

/// <summary>
/// Interface cho ArticleComment Repository
/// </summary>
public interface IArticleCommentRepository : IRepository<ArticleComment>
{
    /// <summary>
    /// Lấy các bình luận theo ArticleId
    /// </summary>
    Task<IEnumerable<ArticleComment>> GetByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lấy các reply của một comment
    /// </summary>
    Task<IEnumerable<ArticleComment>> GetRepliesByCommentIdAsync(Guid commentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lấy các bình luận theo UserId
    /// </summary>
    Task<IEnumerable<ArticleComment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lấy các bình luận theo trạng thái
    /// </summary>
    Task<IEnumerable<ArticleComment>> GetByStatusAsync(CommentStatus status, CancellationToken cancellationToken = default);
}
