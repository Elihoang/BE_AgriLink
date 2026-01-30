using AgriLink_DH.Domain.Common;
using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

/// <summary>
/// Interface cho Article Repository
/// </summary>
public interface IArticleRepository : IRepository<Article>
{
    /// <summary>
    /// Lấy Article theo Slug
    /// </summary>
    Task<Article?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lấy các bài viết đã xuất bản
    /// </summary>
    Task<IEnumerable<Article>> GetPublishedArticlesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lấy các bài viết theo CategoryId
    /// </summary>
    Task<IEnumerable<Article>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lấy các bài viết theo AuthorId
    /// </summary>
    Task<IEnumerable<Article>> GetByAuthorIdAsync(Guid authorId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lấy các bài viết nổi bật
    /// </summary>
    Task<IEnumerable<Article>> GetFeaturedArticlesAsync(int count, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lấy các bài viết theo trạng thái
    /// </summary>
    Task<IEnumerable<Article>> GetByStatusAsync(ArticleStatus status, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Tăng view count
    /// </summary>
    Task IncrementViewCountAsync(Guid articleId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Kiểm tra slug đã tồn tại chưa
    /// </summary>
    Task<bool> SlugExistsAsync(string slug, Guid? excludeArticleId = null, CancellationToken cancellationToken = default);
}
