using AgriLink_DH.Domain.Common;
using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

/// <summary>
/// Interface cho ArticleCategory Repository
/// </summary>
public interface IArticleCategoryRepository : IRepository<ArticleCategory>
{
    /// <summary>
    /// Lấy ArticleCategory theo Code (enum)
    /// </summary>
    Task<ArticleCategory?> GetByCodeAsync(ArticleCategoryType code, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Kiểm tra ArticleCategory có tồn tại theo Code không
    /// </summary>
    Task<bool> ExistsByCodeAsync(ArticleCategoryType code, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lấy các danh mục đang kích hoạt
    /// </summary>
    Task<IEnumerable<ArticleCategory>> GetActiveCategories(CancellationToken cancellationToken = default);
}
