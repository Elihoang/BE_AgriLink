using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.Article;

namespace AgriLink_DH.Core.Validations;

public class ArticleValidator
{
    private readonly IArticleRepository _articleRepository;
    private readonly IArticleCategoryRepository _categoryRepository;
    private readonly IArticleAuthorRepository _authorRepository;

    public ArticleValidator(
        IArticleRepository articleRepository,
        IArticleCategoryRepository categoryRepository,
        IArticleAuthorRepository authorRepository)
    {
        _articleRepository = articleRepository;
        _categoryRepository = categoryRepository;
        _authorRepository = authorRepository;
    }

    public async Task ValidateCreateAsync(CreateArticleDto dto, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy danh mục với ID: {dto.CategoryId}");
        }

        var author = await _authorRepository.GetByIdAsync(dto.AuthorId, cancellationToken);
        if (author == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy tác giả với ID: {dto.AuthorId}");
        }
    }

    public async Task ValidateUpdateAsync(Article? article, Guid id, UpdateArticleDto dto, CancellationToken cancellationToken = default)
    {
        if (article == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bài viết với ID: {id}");
        }

        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy danh mục với ID: {dto.CategoryId}");
        }

        var author = await _authorRepository.GetByIdAsync(dto.AuthorId, cancellationToken);
        if (author == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy tác giả với ID: {dto.AuthorId}");
        }
    }

    public async Task ValidateDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _articleRepository.ExistsAsync(a => a.Id == id, cancellationToken);
        if (!exists)
        {
            throw new KeyNotFoundException($"Không tìm thấy bài viết với ID: {id}");
        }
    }

    public void ValidatePublish(Article? article, Guid id)
    {
        if (article == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bài viết với ID: {id}");
        }
    }
}
