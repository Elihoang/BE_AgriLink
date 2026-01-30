using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.ArticleAuthor;
using System.Text.Json;

namespace AgriLink_DH.Core.Services;

/// <summary>
/// Service xử lý business logic cho ArticleAuthor
/// </summary>
public class ArticleAuthorService
{
    private readonly IArticleAuthorRepository _authorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArticleAuthorService(
        IArticleAuthorRepository authorRepository,
        IUnitOfWork unitOfWork)
    {
        _authorRepository = authorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ArticleAuthorDto>> GetAllAuthorsAsync(CancellationToken cancellationToken = default)
    {
        var authors = await _authorRepository.GetAllAsync(cancellationToken);
        return authors.Select(MapToDto);
    }

    public async Task<IEnumerable<ArticleAuthorDto>> GetVerifiedAuthorsAsync(CancellationToken cancellationToken = default)
    {
        var authors = await _authorRepository.GetVerifiedAuthorsAsync(cancellationToken);
        return authors.Select(MapToDto);
    }

    public async Task<IEnumerable<ArticleAuthorDto>> GetActiveAuthorsAsync(CancellationToken cancellationToken = default)
    {
        var authors = await _authorRepository.GetActiveAuthorsAsync(cancellationToken);
        return authors.Select(MapToDto);
    }

    public async Task<ArticleAuthorDto?> GetAuthorByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var author = await _authorRepository.GetByIdAsync(id, cancellationToken);
        return author != null ? MapToDto(author) : null;
    }

    public async Task<ArticleAuthorDto?> GetAuthorByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var author = await _authorRepository.GetByEmailAsync(email, cancellationToken);
        return author != null ? MapToDto(author) : null;
    }

    public async Task<ArticleAuthorDto> CreateAuthorAsync(CreateArticleAuthorDto dto, CancellationToken cancellationToken = default)
    {
        // Kiểm tra trùng email nếu có
        if (!string.IsNullOrEmpty(dto.Email))
        {
            var existingAuthor = await _authorRepository.GetByEmailAsync(dto.Email, cancellationToken);
            if (existingAuthor != null)
            {
                throw new InvalidOperationException($"Email '{dto.Email}' đã được sử dụng bởi tác giả khác");
            }
        }

        var author = new ArticleAuthor
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Title = dto.Title,
            Organization = dto.Organization,
            Email = dto.Email,
            Phone = dto.Phone,
            AvatarUrl = dto.AvatarUrl,
            Bio = dto.Bio,
            IsVerified = dto.IsVerified,
            Specialties = dto.Specialties != null ? JsonSerializer.Serialize(dto.Specialties) : null,
            SocialLinks = dto.SocialLinks != null ? JsonSerializer.Serialize(dto.SocialLinks) : null,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _authorRepository.AddAsync(author, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(author);
    }

    public async Task<ArticleAuthorDto> UpdateAuthorAsync(Guid id, UpdateArticleAuthorDto dto, CancellationToken cancellationToken = default)
    {
        var author = await _authorRepository.GetByIdAsync(id, cancellationToken);
        if (author == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy tác giả với ID: {id}");
        }

        // Kiểm tra trùng email nếu thay đổi
        if (!string.IsNullOrEmpty(dto.Email) && dto.Email != author.Email)
        {
            var existingAuthor = await _authorRepository.GetByEmailAsync(dto.Email, cancellationToken);
            if (existingAuthor != null)
            {
                throw new InvalidOperationException($"Email '{dto.Email}' đã được sử dụng bởi tác giả khác");
            }
        }

        author.Name = dto.Name;
        author.Title = dto.Title;
        author.Organization = dto.Organization;
        author.Email = dto.Email;
        author.Phone = dto.Phone;
        author.AvatarUrl = dto.AvatarUrl;
        author.Bio = dto.Bio;
        author.IsVerified = dto.IsVerified;
        author.Specialties = dto.Specialties != null ? JsonSerializer.Serialize(dto.Specialties) : null;
        author.SocialLinks = dto.SocialLinks != null ? JsonSerializer.Serialize(dto.SocialLinks) : null;
        author.IsActive = dto.IsActive;
        author.UpdatedAt = DateTime.UtcNow;

        _authorRepository.Update(author);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(author);
    }

    public async Task<bool> DeleteAuthorAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _authorRepository.ExistsAsync(a => a.Id == id, cancellationToken);
        if (!exists)
        {
            throw new KeyNotFoundException($"Không tìm thấy tác giả với ID: {id}");
        }

        var result = await _authorRepository.RemoveByIdAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }

    private static ArticleAuthorDto MapToDto(ArticleAuthor author)
    {
        return new ArticleAuthorDto
        {
            Id = author.Id,
            Name = author.Name,
            Title = author.Title,
            Organization = author.Organization,
            Email = author.Email,
            Phone = author.Phone,
            AvatarUrl = author.AvatarUrl,
            Bio = author.Bio,
            IsVerified = author.IsVerified,
            Specialties = !string.IsNullOrEmpty(author.Specialties)
                ? JsonSerializer.Deserialize<List<string>>(author.Specialties) ?? new List<string>()
                : new List<string>(),
            SocialLinks = !string.IsNullOrEmpty(author.SocialLinks)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(author.SocialLinks) ?? new Dictionary<string, string>()
                : new Dictionary<string, string>(),
            IsActive = author.IsActive,
            CreatedAt = author.CreatedAt
        };
    }
}
