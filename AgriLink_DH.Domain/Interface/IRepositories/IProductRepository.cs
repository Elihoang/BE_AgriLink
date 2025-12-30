using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

/// <summary>
/// Interface cho Product Repository - kế thừa IRepository<Product> và thêm methods riêng
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    // Specific methods cho Product ngoài các methods generic từ IRepository<Product>
    
    /// <summary>
    /// Lấy Product theo Code
    /// </summary>
    Task<Product?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Kiểm tra Product có tồn tại theo Code không
    /// </summary>
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
}
