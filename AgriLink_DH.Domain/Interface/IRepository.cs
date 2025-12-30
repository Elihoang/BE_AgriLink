using System.Linq.Expressions;

namespace AgriLink_DH.Domain.Interface;

/// <summary>
/// Generic repository interface với các phương thức CRUD cơ bản
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : class
{
    // Read operations
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

    // Create operations
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    // Update operations
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);

    // Delete operations
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    Task<bool> RemoveByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

