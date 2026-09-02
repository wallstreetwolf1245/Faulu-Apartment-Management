using FauluApartmentAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FauluApartmentAPI.Data.Repositories;

/// <summary>
/// Lease repository interface with specialized queries
/// </summary>
public interface ILeaseRepository : IRepository<Lease>
{
    Task<IEnumerable<Lease>> GetActiveLeasesByUnitAsync(int unitId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lease>> GetLeasesByTenantAsync(int tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lease>> GetExpiringLeasesAsync(int daysUntilExpiry, CancellationToken cancellationToken = default);
    Task<Lease?> GetCurrentLeaseByUnitAsync(int unitId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Lease repository implementation
/// </summary>
public class LeaseRepository : Repository<Lease>, ILeaseRepository
{
    public LeaseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Lease>> GetActiveLeasesByUnitAsync(int unitId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(l => l.UnitId == unitId && l.Status == "Active")
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Lease>> GetLeasesByTenantAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(l => l.TenantId == tenantId)
            .OrderByDescending(l => l.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Lease>> GetExpiringLeasesAsync(int daysUntilExpiry, CancellationToken cancellationToken = default)
    {
        var futureDate = DateTime.UtcNow.AddDays(daysUntilExpiry);
        return await _dbSet
            .Where(l => l.Status == "Active" && l.EndDate <= futureDate && l.EndDate > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<Lease?> GetCurrentLeaseByUnitAsync(int unitId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(l => l.UnitId == unitId && l.Status == "Active" && l.StartDate <= DateTime.UtcNow && l.EndDate >= DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
