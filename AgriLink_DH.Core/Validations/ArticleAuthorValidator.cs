using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.ArticleAuthor;

namespace AgriLink_DH.Core.Validations;

public class ArticleAuthorValidator
{
    private readonly IArticleAuthorRepository _authorRepository;

    public ArticleAuthorValidator(IArticleAuthorRepository authorRepository)
    {
        _authorRepository = authorRepository;
    }

    public async Task ValidateCreateAuthorAsync(CreateArticleAuthorDto dto, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(dto.Email))
        {
            var existingAuthor = await _authorRepository.GetByEmailAsync(dto.Email, cancellationToken);
            if (existingAuthor != null)
            {
                throw new InvalidOperationException($"Email '{dto.Email}' đã được sử dụng bởi tác giả khác");
            }
        }
    }

    public async Task ValidateUpdateAuthorAsync(ArticleAuthor? author, UpdateArticleAuthorDto dto, Guid id, CancellationToken cancellationToken = default)
    {
        if (author == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy tác giả với ID: {id}");
        }

        if (!string.IsNullOrEmpty(dto.Email) && dto.Email != author.Email)
        {
            var existingAuthor = await _authorRepository.GetByEmailAsync(dto.Email, cancellationToken);
            if (existingAuthor != null)
            {
                throw new InvalidOperationException($"Email '{dto.Email}' đã được sử dụng bởi tác giả khác");
            }
        }
    }

    public void ValidateDeleteAuthor(bool exists, Guid id)
    {
        if (!exists)
        {
            throw new KeyNotFoundException($"Không tìm thấy tác giả với ID: {id}");
        }
    }
}
