using AgriLink_DH.Infrastructure.Data;
using AgriLink_DH.Domain.Common;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Infrastructure.Repositories;

/// <summary>
/// ArticleComment Repository Implementation
/// </summary>
public class ArticleCommentRepository : BaseRepository<ArticleComment>, IArticleCommentRepository
{
    public ArticleCommentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ArticleComment>> GetByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.Replies)
            .Where(c => c.ArticleId == articleId && c.ParentCommentId == null && c.Status == CommentStatus.Active)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ArticleComment>> GetRepliesByCommentIdAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(c => c.User)
            .Where(c => c.ParentCommentId == commentId && c.Status == CommentStatus.Active)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ArticleComment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(c => c.Article)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ArticleComment>> GetByStatusAsync(CommentStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.Article)
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

