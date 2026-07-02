using AgriLink_DH.Domain.Common;

using AgriLink_DH.Domain.Models.Base;

namespace AgriLink_DH.Domain.Models;

/// <summary>
/// Bài viết tri thức nông nghiệp
/// Chứa hướng dẫn kỹ thuật, kiến thức chuyên môn, tin tức thị trường,...
/// </summary>
public class Article : BaseEntity
{

    public Guid CategoryId { get; private set; }
    public virtual ArticleCategory? Category { get; private set; }

    public Guid AuthorId { get; private set; }
    public virtual ArticleAuthor? Author { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Content { get; private set; }
    public string? ThumbnailUrl { get; private set; }

    public string? Tags { get; private set; } // JSON
    public string? Hashtags { get; private set; } // JSON

    public int ReadTime { get; private set; } = 5;
    public string? AudioUrl { get; private set; }
    public int? AudioDuration { get; private set; }
    public string? VideoUrl { get; private set; }

    public int ViewCount { get; private set; } = 0;
    public int LikeCount { get; private set; } = 0;
    public int CommentCount { get; private set; } = 0;
    public int ShareCount { get; private set; } = 0;

    public ArticleStatus Status { get; private set; } = ArticleStatus.Draft;
    public bool IsFeatured { get; private set; } = false;
    public bool AllowComments { get; private set; } = true;

    public DateTime? PublishedAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public Guid? CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public string? SeoMetadata { get; private set; } // JSON

    public virtual ICollection<ArticleComment> Comments { get; private set; } = new List<ArticleComment>();
    public virtual ICollection<ArticleLike> Likes { get; private set; } = new List<ArticleLike>();

    protected Article() { }

    /// <summary>
    /// Hàm khởi tạo chuẩn Rich Domain Model
    /// </summary>
    public Article(string title, string slug, Guid categoryId, Guid authorId, Guid? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Tiêu đề không được để trống", nameof(title));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug không được để trống", nameof(slug));
        if (categoryId == Guid.Empty)
            throw new ArgumentException("Danh mục không hợp lệ", nameof(categoryId));
        if (authorId == Guid.Empty)
            throw new ArgumentException("Tác giả không hợp lệ", nameof(authorId));

        Title = title.Trim();
        Slug = slug.Trim();
        CategoryId = categoryId;
        AuthorId = authorId;
        Status = ArticleStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
        AllowComments = true;
    }

    // --- BEHAVIORS ---

    public void UpdateContent(string title, string slug, string? description, string? content, string? thumbnailUrl, int readTime, Guid? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Tiêu đề không được để trống", nameof(title));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug không được để trống", nameof(slug));

        Title = title.Trim();
        Slug = slug.Trim();
        Description = description?.Trim();
        Content = content;
        ThumbnailUrl = thumbnailUrl?.Trim();
        ReadTime = readTime > 0 ? readTime : 1;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeCategory(Guid categoryId, Guid? updatedBy = null)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException("Danh mục không hợp lệ", nameof(categoryId));

        CategoryId = categoryId;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeAuthor(Guid authorId, Guid? updatedBy = null)
    {
        if (authorId == Guid.Empty)
            throw new ArgumentException("Tác giả không hợp lệ", nameof(authorId));

        AuthorId = authorId;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMedia(string? audioUrl, int? audioDuration, string? videoUrl)
    {
        AudioUrl = audioUrl?.Trim();
        AudioDuration = audioDuration;
        VideoUrl = videoUrl?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTags(string? tagsJson, string? hashtagsJson)
    {
        Tags = tagsJson;
        Hashtags = hashtagsJson;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSeo(string? seoMetadata)
    {
        SeoMetadata = seoMetadata;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish(Guid? publishedBy = null)
    {
        if (Status != ArticleStatus.Published)
        {
            Status = ArticleStatus.Published;
            PublishedAt = DateTime.UtcNow;
            UpdatedBy = publishedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RevertToDraft(Guid? updatedBy = null)
    {
        Status = ArticleStatus.Draft;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetFeatured(bool isFeatured, Guid? updatedBy = null)
    {
        IsFeatured = isFeatured;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAllowComments(bool allowComments, Guid? updatedBy = null)
    {
        AllowComments = allowComments;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    // --- METRICS UPDATE ---

    public void IncrementViewCount()
    {
        ViewCount++;
    }

    public void IncrementLikeCount()
    {
        LikeCount++;
    }

    public void DecrementLikeCount()
    {
        if (LikeCount > 0)
            LikeCount--;
    }

    public void IncrementCommentCount()
    {
        CommentCount++;
    }

    public void DecrementCommentCount()
    {
        if (CommentCount > 0)
            CommentCount--;
    }

    public void IncrementShareCount()
    {
        ShareCount++;
    }
}
