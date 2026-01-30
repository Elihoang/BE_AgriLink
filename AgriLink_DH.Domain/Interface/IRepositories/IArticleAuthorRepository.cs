using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

/// <summary>
/// Interface cho ArticleAuthor Repository
/// </summary>
public interface IArticleAuthorRepository : IRepository<ArticleAuthor>
{
    /// <summary>
    /// Lấy ArticleAuthor theo Email
    /// </summary>
    Task<ArticleAuthor?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lấy các tác giả đã xác minh
    /// </summary>
    Task<IEnumerable<ArticleAuthor>> GetVerifiedAuthorsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lấy các tác giả đang kích hoạt
    /// </summary>
    Task<IEnumerable<ArticleAuthor>> GetActiveAuthorsAsync(CancellationToken cancellationToken = default);
}
