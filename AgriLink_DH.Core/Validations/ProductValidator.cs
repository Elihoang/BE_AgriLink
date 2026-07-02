using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.Product;

namespace AgriLink_DH.Core.Validations;

public class ProductValidator
{
    private readonly IProductRepository _productRepository;

    public ProductValidator(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task ValidateCreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(dto.Code))
        {
            var exists = await _productRepository.ExistsByCodeAsync(dto.Code, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"Mã sản phẩm '{dto.Code}' đã tồn tại");
            }
        }
    }

    public async Task ValidateUpdateAsync(Product? product, UpdateProductDto dto, Guid id, CancellationToken cancellationToken = default)
    {
        if (product == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy sản phẩm với ID: {id}");
        }

        if (!string.IsNullOrEmpty(dto.Code) && dto.Code != product.Code)
        {
            var exists = await _productRepository.ExistsByCodeAsync(dto.Code, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"Mã sản phẩm '{dto.Code}' đã tồn tại");
            }
        }
    }

    public void ValidateUpdateImage(Product? product, Guid id)
    {
        if (product == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy sản phẩm với ID: {id}");
        }
    }

    public void ValidateDelete(bool exists, Guid id)
    {
        if (!exists)
        {
            throw new KeyNotFoundException($"Không tìm thấy sản phẩm với ID: {id}");
        }
    }
}
