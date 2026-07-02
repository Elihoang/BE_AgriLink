using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Core.Validations;
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
    private readonly ProductValidator _validator;

    public ProductService(
        IProductRepository productRepository, 
        IUnitOfWork unitOfWork,
        ProductValidator validator)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
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
        await _validator.ValidateCreateAsync(dto, cancellationToken);

        var product = new Product(dto.Name, dto.Unit, dto.Code, dto.ImageUrl);

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return MapToDto(product);
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        await _validator.ValidateUpdateAsync(product, dto, id, cancellationToken);

        product.UpdateDetails(dto.Name, dto.Unit, dto.Code, dto.ImageUrl);

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return MapToDto(product);
    }

    public async Task UpdateProductImageAsync(Guid id, string imageUrl, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        _validator.ValidateUpdateImage(product, id);

        product.UpdateDetails(product.Name, product.Unit, product.Code, imageUrl);
        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _productRepository.ExistsAsync(p => p.Id == id, cancellationToken);
        _validator.ValidateDelete(exists, id);

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
