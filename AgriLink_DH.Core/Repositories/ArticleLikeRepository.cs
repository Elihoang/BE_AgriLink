using AgriLink_DH.Core.Configurations;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Core.Repositories;

/// <summary>
/// ArticleLike Repository Implementation
/// </summary>
public class ArticleLikeRepository : BaseRepository<ArticleLike>, IArticleLikeRepository
{
    public ArticleLikeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> HasUserLikedArticleAsync(Guid articleId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await ExistsAsync(l => l.ArticleId == articleId && l.UserId == userId, cancellationToken);
    }

    public async Task<ArticleLike?> GetByArticleAndUserAsync(Guid articleId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await FirstOrDefaultAsync(l => l.ArticleId == articleId && l.UserId == userId, cancellationToken);
    }

    public async Task<int> GetLikeCountByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(l => l.ArticleId == articleId)
            .CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<ArticleLike>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(l => l.Article)
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
