using FauluApartmentAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FauluApartmentAPI.Data.Repositories;

/// <summary>
/// Unit repository interface with specialized queries
/// </summary>
public interface IUnitRepository : IRepository<Unit>
{
    Task<IEnumerable<Unit>> GetUnitsByBuildingAsync(int buildingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Unit>> GetVacantUnitsAsync(int buildingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Unit>> GetOccupiedUnitsAsync(int buildingId, CancellationToken cancellationToken = default);
    Task<Unit?> GetByUnitNumberAsync(int buildingId, string unitNumber, CancellationToken cancellationToken = default);
    Task<int> GetVacancyCountAsync(int buildingId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Unit repository implementation
/// </summary>
public class UnitRepository : Repository<Unit>, IUnitRepository
{
    public UnitRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Unit>> GetUnitsByBuildingAsync(int buildingId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.BuildingId == buildingId)
            .OrderBy(u => u.FloorNumber)
            .ThenBy(u => u.UnitNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Unit>> GetVacantUnitsAsync(int buildingId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.BuildingId == buildingId && u.Status == "Vacant")
            .OrderBy(u => u.FloorNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Unit>> GetOccupiedUnitsAsync(int buildingId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.BuildingId == buildingId && u.Status == "Occupied")
            .OrderBy(u => u.FloorNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<Unit?> GetByUnitNumberAsync(int buildingId, string unitNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.BuildingId == buildingId && u.UnitNumber == unitNumber, cancellationToken);
    }

    public async Task<int> GetVacancyCountAsync(int buildingId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(u => u.BuildingId == buildingId && u.Status == "Vacant", cancellationToken);
    }
}
