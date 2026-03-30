using AgriLink_DH.Infrastructure.Data;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Infrastructure.Repositories;

/// <summary>
/// ArticleAuthor Repository Implementation
/// </summary>
public class ArticleAuthorRepository : BaseRepository<ArticleAuthor>, IArticleAuthorRepository
{
    public ArticleAuthorRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<ArticleAuthor>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<ArticleAuthor?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await FirstOrDefaultAsync(a => a.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<ArticleAuthor>> GetVerifiedAuthorsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(a => a.IsVerified && a.IsActive)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ArticleAuthor>> GetActiveAuthorsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(a => a.IsActive)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }
}

