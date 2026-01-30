using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

/// <summary>
/// Interface cho ArticleLike Repository
/// </summary>
public interface IArticleLikeRepository : IRepository<ArticleLike>
{
    /// <summary>
    /// Kiểm tra user đã like bài viết chưa
    /// </summary>
    Task<bool> HasUserLikedArticleAsync(Guid articleId, Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lấy like theo ArticleId và UserId
    /// </summary>
    Task<ArticleLike?> GetByArticleAndUserAsync(Guid articleId, Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lấy số lượng like của một bài viết
    /// </summary>
    Task<int> GetLikeCountByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lấy các bài viết user đã like
    /// </summary>
    Task<IEnumerable<ArticleLike>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
