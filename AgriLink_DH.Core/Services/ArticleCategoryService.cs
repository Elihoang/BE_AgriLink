using AgriLink_DH.Domain.Common;
using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.ArticleCategory;

namespace AgriLink_DH.Core.Services;

/// <summary>
/// Service xử lý business logic cho ArticleCategory
/// </summary>
public class ArticleCategoryService
{
    private readonly IArticleCategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArticleCategoryService(
        IArticleCategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ArticleCategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        return categories.Select(MapToDto);
    }

    public async Task<IEnumerable<ArticleCategoryDto>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetActiveCategories(cancellationToken);
        return categories.Select(MapToDto);
    }

    public async Task<ArticleCategoryDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        return category != null ? MapToDto(category) : null;
    }

    public async Task<ArticleCategoryDto?> GetCategoryByCodeAsync(ArticleCategoryType code, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByCodeAsync(code, cancellationToken);
        return category != null ? MapToDto(category) : null;
    }

    public async Task<ArticleCategoryDto> CreateCategoryAsync(CreateArticleCategoryDto dto, CancellationToken cancellationToken = default)
    {
        // Kiểm tra trùng code
        var exists = await _categoryRepository.ExistsByCodeAsync(dto.Code, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Danh mục với mã '{dto.Code}' đã tồn tại");
        }

        var category = new ArticleCategory
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description,
            Icon = dto.Icon,
            Color = dto.Color,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(category);
    }

    public async Task<ArticleCategoryDto> UpdateCategoryAsync(Guid id, UpdateArticleCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy danh mục với ID: {id}");
        }

        // Kiểm tra trùng code nếu thay đổi
        if (dto.Code != category.Code)
        {
            var exists = await _categoryRepository.ExistsByCodeAsync(dto.Code, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"Danh mục với mã '{dto.Code}' đã tồn tại");
            }
        }

        category.Name = dto.Name;
        category.Code = dto.Code;
        category.Description = dto.Description;
        category.Icon = dto.Icon;
        category.Color = dto.Color;
        category.DisplayOrder = dto.DisplayOrder;
        category.IsActive = dto.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        _categoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(category);
    }

    public async Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _categoryRepository.ExistsAsync(c => c.Id == id, cancellationToken);
        if (!exists)
        {
            throw new KeyNotFoundException($"Không tìm thấy danh mục với ID: {id}");
        }

        var result = await _categoryRepository.RemoveByIdAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }

    private static ArticleCategoryDto MapToDto(ArticleCategory category)
    {
        return new ArticleCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Code = category.Code,
            Description = category.Description,
            Icon = category.Icon,
            Color = category.Color,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}
