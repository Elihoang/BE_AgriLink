using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.ArticleCategory;

namespace AgriLink_DH.Core.Validations;

public class ArticleCategoryValidator
{
    private readonly IArticleCategoryRepository _categoryRepository;

    public ArticleCategoryValidator(IArticleCategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task ValidateCreateAsync(CreateArticleCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var exists = await _categoryRepository.ExistsByCodeAsync(dto.Code, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Danh mục với mã '{dto.Code}' đã tồn tại");
        }
    }

    public async Task ValidateUpdateAsync(ArticleCategory? category, Guid id, UpdateArticleCategoryDto dto, CancellationToken cancellationToken = default)
    {
        if (category == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy danh mục với ID: {id}");
        }

        if (dto.Code != category.Code)
        {
            var exists = await _categoryRepository.ExistsByCodeAsync(dto.Code, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"Danh mục với mã '{dto.Code}' đã tồn tại");
            }
        }
    }

    public async Task ValidateDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _categoryRepository.ExistsAsync(c => c.Id == id, cancellationToken);
        if (!exists)
        {
            throw new KeyNotFoundException($"Không tìm thấy danh mục với ID: {id}");
        }
    }
}
