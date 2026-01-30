using AgriLink_DH.Core.Configurations;
using AgriLink_DH.Domain.Common;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Core.Repositories;

/// <summary>
/// ArticleCategory Repository Implementation
/// </summary>
public class ArticleCategoryRepository : BaseRepository<ArticleCategory>, IArticleCategoryRepository
{
    public ArticleCategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<ArticleCategory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<ArticleCategory?> GetByCodeAsync(ArticleCategoryType code, CancellationToken cancellationToken = default)
    {
        return await FirstOrDefaultAsync(c => c.Code == code, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(ArticleCategoryType code, CancellationToken cancellationToken = default)
    {
        return await ExistsAsync(c => c.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<ArticleCategory>> GetActiveCategories(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}
