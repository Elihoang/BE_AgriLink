using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Core.Validations;
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
    private readonly ArticleAuthorValidator _validator;

    public ArticleAuthorService(
        IArticleAuthorRepository authorRepository,
        IUnitOfWork unitOfWork,
        ArticleAuthorValidator validator)
    {
        _authorRepository = authorRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
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
        await _validator.ValidateCreateAuthorAsync(dto, cancellationToken);

        var author = new ArticleAuthor(dto.Name, dto.Title, dto.Organization, dto.Email, dto.Phone);
        
        author.UpdateProfile(
            dto.Bio, 
            dto.AvatarUrl, 
            dto.SocialLinks != null ? JsonSerializer.Serialize(dto.SocialLinks) : null,
            dto.Specialties != null ? JsonSerializer.Serialize(dto.Specialties) : null
        );

        if (dto.IsVerified) 
            author.Verify();

        if (!dto.IsActive) 
            author.Deactivate();

        await _authorRepository.AddAsync(author, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(author);
    }

    public async Task<ArticleAuthorDto> UpdateAuthorAsync(Guid id, UpdateArticleAuthorDto dto, CancellationToken cancellationToken = default)
    {
        var author = await _authorRepository.GetByIdAsync(id, cancellationToken);
        await _validator.ValidateUpdateAuthorAsync(author, dto, id, cancellationToken);

        author!.UpdateBasicInfo(dto.Name, dto.Title, dto.Organization);
        author.UpdateContactInfo(dto.Email, dto.Phone);
        
        author.UpdateProfile(
            dto.Bio, 
            dto.AvatarUrl,
            dto.SocialLinks != null ? JsonSerializer.Serialize(dto.SocialLinks) : null,
            dto.Specialties != null ? JsonSerializer.Serialize(dto.Specialties) : null
        );

        if (dto.IsVerified) author.Verify();
        else author.RevokeVerification();

        if (dto.IsActive) author.Activate();
        else author.Deactivate();

        _authorRepository.Update(author);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(author);
    }

    public async Task<bool> DeleteAuthorAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _authorRepository.ExistsAsync(a => a.Id == id, cancellationToken);
        _validator.ValidateDeleteAuthor(exists, id);

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
