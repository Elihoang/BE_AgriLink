using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Share.DTOs.Product;

namespace AgriLink_DH.Core.Services;

/// <summary>
/// Service xử lý business logic cho Product
/// </summary>
public class ProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);
        return products.Select(MapToDto);
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        return product != null ? MapToDto(product) : null;
    }

    public async Task<ProductDto?> GetProductByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByCodeAsync(code, cancellationToken);
        return product != null ? MapToDto(product) : null;
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        // Kiểm tra trùng code nếu có
        if (!string.IsNullOrEmpty(dto.Code))
        {
            var exists = await _productRepository.ExistsByCodeAsync(dto.Code, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"Mã sản phẩm '{dto.Code}' đã tồn tại");
            }
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Unit = dto.Unit,
            Code = dto.Code,
            ImageUrl = dto.ImageUrl
        };

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return MapToDto(product);
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy sản phẩm với ID: {id}");
        }

        // Kiểm tra trùng code nếu thay đổi code
        if (!string.IsNullOrEmpty(dto.Code) && dto.Code != product.Code)
        {
            var exists = await _productRepository.ExistsByCodeAsync(dto.Code, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"Mã sản phẩm '{dto.Code}' đã tồn tại");
            }
        }

        product.Name = dto.Name;
        product.Unit = dto.Unit;
        product.Code = dto.Code;
        product.ImageUrl = dto.ImageUrl;

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return MapToDto(product);
    }

    public async Task UpdateProductImageAsync(Guid id, string imageUrl, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy sản phẩm với ID: {id}");
        }

        product.ImageUrl = imageUrl;
        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _productRepository.ExistsAsync(p => p.Id == id, cancellationToken);
        if (!exists)
        {
            throw new KeyNotFoundException($"Không tìm thấy sản phẩm với ID: {id}");
        }

        var result = await _productRepository.RemoveByIdAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return result;
    }

    // Helper method để map từ Entity sang DTO
    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Unit = product.Unit,
            Code = product.Code,
            ImageUrl = product.ImageUrl
        };
    }
}
