using System.Linq.Expressions;
using FauluApartmentAPI.Data.Entities;

namespace FauluApartmentAPI.Data.Repositories;

/// <summary>
/// Generic repository interface for CRUD operations
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    // ── Queryable ──────────────────────────────────────────────────────────
    IQueryable<T> QueryableNoTracking();

    // ── Read ───────────────────────────────────────────────────────────────
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    // ── Write ──────────────────────────────────────────────────────────────
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}