using AgriLink_DH.Core.Helpers;
using AgriLink_DH.Domain.Common;
using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.Article;
using System.Text.Json;

namespace AgriLink_DH.Core.Services;

/// <summary>
/// Service xử lý business logic cho Article
/// </summary>
public class ArticleService : BaseCachedService
{
    private readonly IArticleRepository _articleRepository;
    private readonly IArticleCategoryRepository _categoryRepository;
    private readonly IArticleAuthorRepository _authorRepository;
    private readonly IUnitOfWork _unitOfWork;

    // Cache keys & expiration
    private const string CACHE_KEY_PUBLISHED = "articles:published";
    private const string CACHE_KEY_FEATURED = "articles:featured";
    private const string CACHE_KEY_ARTICLE_BY_ID = "article:id:{0}";
    private const string CACHE_KEY_ARTICLE_BY_SLUG = "article:slug:{0}";
    private const int CACHE_MINUTES_LIST = 15; // 15 phút cho danh sách
    private const int CACHE_MINUTES_DETAIL = 60; // 60 phút cho chi tiết bài viết

    public ArticleService(
        IArticleRepository articleRepository,
        IArticleCategoryRepository categoryRepository,
        IArticleAuthorRepository authorRepository,
        IUnitOfWork unitOfWork,
        RedisService redisService)
        : base(redisService) // Pass to base class
    {
        _articleRepository = articleRepository;
        _categoryRepository = categoryRepository;
        _authorRepository = authorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ArticleDto>> GetAllArticlesAsync(CancellationToken cancellationToken = default)
    {
        var articles = await _articleRepository.GetAllAsync(cancellationToken);
        return articles.Select(MapToDto);
    }

    public async Task<IEnumerable<ArticleListItemDto>> GetPublishedArticlesAsync(CancellationToken cancellationToken = default)
    {
        // Check cache trước
        var cachedData = await RedisService.GetAsync<List<ArticleListItemDto>>(CACHE_KEY_PUBLISHED);
        if (cachedData != null)
        {
            return cachedData;
        }

        // Không có cache -> fetch từ DB
        var articles = await _articleRepository.GetPublishedArticlesAsync(cancellationToken);
        var result = articles.Select(MapToListItemDto).ToList();

        // Save vào cache 15 phút
        await RedisService.SetAsync(CACHE_KEY_PUBLISHED, result, TimeSpan.FromMinutes(CACHE_MINUTES_LIST));

        return result;
    }

    public async Task<IEnumerable<ArticleListItemDto>> GetFeaturedArticlesAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CACHE_KEY_FEATURED}:{count}";
        
        // Check cache
        var cachedData = await RedisService.GetAsync<List<ArticleListItemDto>>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }

        // Fetch từ DB
        var articles = await _articleRepository.GetFeaturedArticlesAsync(count, cancellationToken);
        var result = articles.Select(MapToListItemDto).ToList();

        // Cache 15 phút
        await RedisService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(CACHE_MINUTES_LIST));

        return result;
    }

    public async Task<IEnumerable<ArticleDto>> GetArticlesByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var articles = await _articleRepository.GetByCategoryIdAsync(categoryId, cancellationToken);
        return articles.Select(MapToDto);
    }

    public async Task<IEnumerable<ArticleDto>> GetArticlesByAuthorIdAsync(Guid authorId, CancellationToken cancellationToken = default)
    {
        var articles = await _articleRepository.GetByAuthorIdAsync(authorId, cancellationToken);
        return articles.Select(MapToDto);
    }

    public async Task<IEnumerable<ArticleDto>> GetArticlesByStatusAsync(ArticleStatus status, CancellationToken cancellationToken = default)
    {
        var articles = await _articleRepository.GetByStatusAsync(status, cancellationToken);
        return articles.Select(MapToDto);
    }

    public async Task<ArticleDto?> GetArticleByIdAsync(Guid id, bool incrementView = false, CancellationToken cancellationToken = default)
    {
        var article = await _articleRepository.GetByIdAsync(id, cancellationToken);
        if (article == null) return null;

        if (incrementView)
        {
            await _articleRepository.IncrementViewCountAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return MapToDto(article);
    }

    public async Task<ArticleDto?> GetArticleBySlugAsync(string slug, bool incrementView = false, CancellationToken cancellationToken = default)
    {
        var cacheKey = string.Format(CACHE_KEY_ARTICLE_BY_SLUG, slug);

        // Nếu không increment view, check cache
        if (!incrementView)
        {
            var cachedData = await RedisService.GetAsync<ArticleDto>(cacheKey);
            if (cachedData != null)
            {
                return cachedData;
            }
        }

        var article = await _articleRepository.GetBySlugAsync(slug, cancellationToken);
        if (article == null) return null;

        if (incrementView)
        {
            await _articleRepository.IncrementViewCountAsync(article.Id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Invalidate cache vì ViewCount đã thay đổi
            await RedisService.DeleteAsync(cacheKey);
            await RedisService.DeleteAsync(string.Format(CACHE_KEY_ARTICLE_BY_ID, article.Id));
        }

        var result = MapToDto(article);

        // Cache 60 phút nếu KHÔNG increment view
        if (!incrementView)
        {
            await RedisService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(CACHE_MINUTES_DETAIL));
        }

        return result;
    }

    public async Task<ArticleDto> CreateArticleAsync(CreateArticleDto dto, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        // Validate Category exists
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy danh mục với ID: {dto.CategoryId}");
        }

        // Validate Author exists
        var author = await _authorRepository.GetByIdAsync(dto.AuthorId, cancellationToken);
        if (author == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy tác giả với ID: {dto.AuthorId}");
        }

        // Generate slug
        var slug = ArticleHelper.GenerateSlug(dto.Title);
        var slugExists = await _articleRepository.SlugExistsAsync(slug, null, cancellationToken);
        if (slugExists)
        {
            slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";
        }

        var article = new Article
        {
            Id = Guid.NewGuid(),
            CategoryId = dto.CategoryId,
            AuthorId = dto.AuthorId,
            Title = dto.Title,
            Slug = slug,
            Description = dto.Description,
            Content = dto.Content,
            ThumbnailUrl = dto.ThumbnailUrl,
            Tags = dto.Tags != null ? JsonSerializer.Serialize(dto.Tags) : null,
            Hashtags = dto.Hashtags != null ? JsonSerializer.Serialize(dto.Hashtags) : null,
            ReadTime = dto.ReadTime,
            AudioUrl = dto.AudioUrl,
            AudioDuration = dto.AudioDuration,
            VideoUrl = dto.VideoUrl,
            Status = dto.PublishImmediately ? ArticleStatus.Published : ArticleStatus.Draft,
            IsFeatured = dto.IsFeatured,
            AllowComments = dto.AllowComments,
            PublishedAt = dto.PublishImmediately ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdByUserId
        };

        await _articleRepository.AddAsync(article, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache lists (published & featured)
        await InvalidateListCachesAsync();

        // Reload to get navigation properties
        var createdArticle = await _articleRepository.GetByIdAsync(article.Id, cancellationToken);
        return MapToDto(createdArticle!);
    }

    public async Task<ArticleDto> UpdateArticleAsync(Guid id, UpdateArticleDto dto, Guid updatedByUserId, CancellationToken cancellationToken = default)
    {
        var article = await _articleRepository.GetByIdAsync(id, cancellationToken);
        if (article == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bài viết với ID: {id}");
        }

        // Validate Category exists
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy danh mục với ID: {dto.CategoryId}");
        }

        // Validate Author exists
        var author = await _authorRepository.GetByIdAsync(dto.AuthorId, cancellationToken);
        if (author == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy tác giả với ID: {dto.AuthorId}");
        }

        // Check if title changed, regenerate slug
        if (article.Title != dto.Title)
        {
            var newSlug = ArticleHelper.GenerateSlug(dto.Title);
            var slugExists = await _articleRepository.SlugExistsAsync(newSlug, id, cancellationToken);
            if (slugExists)
            {
                newSlug = $"{newSlug}-{Guid.NewGuid().ToString()[..8]}";
            }
            article.Slug = newSlug;
        }

        article.CategoryId = dto.CategoryId;
        article.AuthorId = dto.AuthorId;
        article.Title = dto.Title;
        article.Description = dto.Description;
        article.Content = dto.Content;
        article.ThumbnailUrl = dto.ThumbnailUrl;
        article.Tags = dto.Tags != null ? JsonSerializer.Serialize(dto.Tags) : null;
        article.Hashtags = dto.Hashtags != null ? JsonSerializer.Serialize(dto.Hashtags) : null;
        article.ReadTime = dto.ReadTime;
        article.AudioUrl = dto.AudioUrl;
        article.AudioDuration = dto.AudioDuration;
        article.VideoUrl = dto.VideoUrl;
        article.IsFeatured = dto.IsFeatured;
        article.AllowComments = dto.AllowComments;
        article.UpdatedAt = DateTime.UtcNow;
        article.UpdatedBy = updatedByUserId;

        // Handle status change to Published
        if (article.Status != ArticleStatus.Published && dto.Status == ArticleStatus.Published)
        {
            article.PublishedAt = DateTime.UtcNow;
        }
        article.Status = dto.Status;

        _articleRepository.Update(article);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await InvalidateArticleCacheAsync(article.Id, article.Slug);

        // Reload to get navigation properties
        var updatedArticle = await _articleRepository.GetByIdAsync(article.Id, cancellationToken);
        return MapToDto(updatedArticle!);
    }

    public async Task<bool> DeleteArticleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _articleRepository.ExistsAsync(a => a.Id == id, cancellationToken);
        if (!exists)
        {
            throw new KeyNotFoundException($"Không tìm thấy bài viết với ID: {id}");
        }

        var result = await _articleRepository.RemoveByIdAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate all caches
        await InvalidateListCachesAsync();

        return result;
    }

    public async Task<ArticleDto> PublishArticleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var article = await _articleRepository.GetByIdAsync(id, cancellationToken);
        if (article == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bài viết với ID: {id}");
        }

        article.Status = ArticleStatus.Published;
        article.PublishedAt = DateTime.UtcNow;
        article.UpdatedAt = DateTime.UtcNow;

        _articleRepository.Update(article);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var publishedArticle = await _articleRepository.GetByIdAsync(article.Id, cancellationToken);
        
        // Invalidate cache khi publish
        await InvalidateArticleCacheAsync(article.Id, article.Slug);
        
        return MapToDto(publishedArticle!);
    }

    /// <summary>
    /// Invalidate tất cả cache lists (published, featured)
    /// </summary>
    private async Task InvalidateListCachesAsync()
    {
        await RedisService.DeleteAsync(CACHE_KEY_PUBLISHED);
        // Delete featured với pattern vì có count parameter
        await RedisService.DeleteByPatternAsync($"{CACHE_KEY_FEATURED}:*");
    }

    /// <summary>
    /// Invalidate cache của 1 article cụ thể
    /// </summary>
    private async Task InvalidateArticleCacheAsync(Guid id, string slug)
    {
        await RedisService.DeleteAsync(string.Format(CACHE_KEY_ARTICLE_BY_ID, id));
        await RedisService.DeleteAsync(string.Format(CACHE_KEY_ARTICLE_BY_SLUG, slug));
        // Invalidate lists luôn vì article có thể nằm trong published/featured
        await InvalidateListCachesAsync();
    }

    // ==================== DOMAIN MAPPERS ====================

    private static ArticleDto MapToDto(Article article)
    {
        return new ArticleDto
        {
            Id = article.Id,
            CategoryId = article.CategoryId,
            CategoryName = article.Category?.Name ?? string.Empty,
            CategoryCode = article.Category?.Code ?? default,
            AuthorId = article.AuthorId,
            AuthorName = article.Author?.Name ?? string.Empty,
            AuthorAvatar = article.Author?.AvatarUrl,
            AuthorVerified = article.Author?.IsVerified ?? false,
            AuthorOrganization = article.Author?.Organization,
            Title = article.Title,
            Slug = article.Slug,
            Description = article.Description,
            Content = article.Content,
            ThumbnailUrl = article.ThumbnailUrl,
            Tags = !string.IsNullOrEmpty(article.Tags)
                ? JsonSerializer.Deserialize<List<string>>(article.Tags) ?? new List<string>()
                : new List<string>(),
            Hashtags = !string.IsNullOrEmpty(article.Hashtags)
                ? JsonSerializer.Deserialize<List<string>>(article.Hashtags) ?? new List<string>()
                : new List<string>(),
            ReadTime = article.ReadTime,
            AudioUrl = article.AudioUrl,
            AudioDuration = article.AudioDuration,
            VideoUrl = article.VideoUrl,
            ViewCount = article.ViewCount,
            LikeCount = article.LikeCount,
            CommentCount = article.CommentCount,
            ShareCount = article.ShareCount,
            Status = article.Status,
            IsFeatured = article.IsFeatured,
            AllowComments = article.AllowComments,
            PublishedAt = article.PublishedAt,
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.UpdatedAt
        };
    }

    private static ArticleListItemDto MapToListItemDto(Article article)
    {
        return new ArticleListItemDto
        {
            Id = article.Id,
            Title = article.Title,
            Slug = article.Slug,
            Description = article.Description,
            ThumbnailUrl = article.ThumbnailUrl,
            CategoryName = article.Category?.Name ?? string.Empty,
            AuthorName = article.Author?.Name ?? string.Empty,
            AuthorAvatar = article.Author?.AvatarUrl,
            AuthorVerified = article.Author?.IsVerified ?? false,
            Tags = !string.IsNullOrEmpty(article.Tags)
                ? JsonSerializer.Deserialize<List<string>>(article.Tags) ?? new List<string>()
                : new List<string>(),
            ReadTime = article.ReadTime,
            ViewCount = article.ViewCount,
            LikeCount = article.LikeCount,
            CommentCount = article.CommentCount,
            PublishedAt = article.PublishedAt,
            PublishedTimeAgo = ArticleHelper.GetTimeAgo(article.PublishedAt)
        };
    }
}
