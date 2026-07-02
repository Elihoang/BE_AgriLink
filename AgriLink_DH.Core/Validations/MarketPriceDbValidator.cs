using AgriLink_DH.Domain.Interface.IRepositories;

namespace AgriLink_DH.Core.Validations;

public class MarketPriceDbValidator
{
    private readonly IProductRepository _productRepository;

    public MarketPriceDbValidator(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task ValidateGetProductIdByCodeAsync(string code)
    {
        var product = await _productRepository.GetByCodeAsync(code);
        if (product == null)
            throw new KeyNotFoundException($"Không tìm thấy sản phẩm với code: '{code}'.");
    }
}
