using AgriLink_DH.Domain.Interface;
using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

/// <summary>
/// Interface cho User Repository - kế thừa IRepository<User> và thêm methods riêng
/// </summary>
public interface IUserRepository : IRepository<User>
{
    // Specific methods cho User ngoài các methods generic từ IRepository<User>
    
    /// <summary>
    /// Lấy User theo Username
    /// </summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lấy User theo Email
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lấy User theo Username hoặc Email
    /// </summary>
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Kiểm tra Username có tồn tại không
    /// </summary>
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Kiểm tra Email có tồn tại không
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
}
