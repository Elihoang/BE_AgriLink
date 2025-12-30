using AgriLink_DH.Domain.Interface.IRepositories;

namespace AgriLink_DH.Domain.Interface;

/// <summary>
/// Interface cho Unit of Work pattern - quản lý transactions và repositories
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Repository properties (optional - cho dự án lớn)
    IProductRepository Products { get; }
    // Thêm các repository khác khi có
    // IUserRepository Users { get; }
    // IOrderRepository Orders { get; }

    /// <summary>
    /// Commit tất cả changes vào database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begin transaction
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commit transaction
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback transaction
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
