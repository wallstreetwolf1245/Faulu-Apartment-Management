using FauluApartmentAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FauluApartmentAPI.Data.Repositories;

/// <summary>
/// MaintenanceOrder repository interface with specialized queries
/// </summary>
public interface IMaintenanceOrderRepository : IRepository<MaintenanceOrder>
{
    Task<IEnumerable<MaintenanceOrder>> GetOrdersByBuildingAsync(int buildingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MaintenanceOrder>> GetOpenOrdersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<MaintenanceOrder>> GetOrdersByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<IEnumerable<MaintenanceOrder>> GetUrgentOrdersAsync(CancellationToken cancellationToken = default);
    Task<int> GetOpenOrdersCountAsync(int buildingId, CancellationToken cancellationToken = default);

    // Owner-scoped variants — join through Building so results are limited
    // to orders on buildings the given user owns. Use these instead of the
    // unscoped versions above whenever the caller is a specific landlord.
    Task<IEnumerable<MaintenanceOrder>> GetOpenOrdersByOwnerAsync(int ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MaintenanceOrder>> GetUrgentOrdersByOwnerAsync(int ownerId, CancellationToken cancellationToken = default);
}

/// <summary>
/// MaintenanceOrder repository implementation
/// </summary>
public class MaintenanceOrderRepository : Repository<MaintenanceOrder>, IMaintenanceOrderRepository
{
    public MaintenanceOrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MaintenanceOrder>> GetOrdersByBuildingAsync(int buildingId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(m => m.BuildingId == buildingId)
            .OrderByDescending(m => m.Priority == "Urgent" ? 0 : 1)
            .ThenByDescending(m => m.ReportedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MaintenanceOrder>> GetOpenOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(m => m.Status == "Open" || m.Status == "InProgress")
            .OrderByDescending(m => m.Priority == "Urgent" ? 0 : 1)
            .ThenByDescending(m => m.ReportedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MaintenanceOrder>> GetOrdersByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(m => m.Status == status)
            .OrderByDescending(m => m.ReportedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MaintenanceOrder>> GetUrgentOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(m => m.Priority == "Urgent" && (m.Status == "Open" || m.Status == "InProgress"))
            .OrderByDescending(m => m.ReportedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetOpenOrdersCountAsync(int buildingId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(m => m.BuildingId == buildingId && (m.Status == "Open" || m.Status == "InProgress"), cancellationToken);
    }

    public async Task<IEnumerable<MaintenanceOrder>> GetOpenOrdersByOwnerAsync(int ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(m => m.Building!.OwnerId == ownerId && (m.Status == "Open" || m.Status == "InProgress"))
            .OrderByDescending(m => m.Priority == "Urgent" ? 0 : 1)
            .ThenByDescending(m => m.ReportedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MaintenanceOrder>> GetUrgentOrdersByOwnerAsync(int ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(m => m.Building!.OwnerId == ownerId && m.Priority == "Urgent" && (m.Status == "Open" || m.Status == "InProgress"))
            .OrderByDescending(m => m.ReportedDate)
            .ToListAsync(cancellationToken);
    }
}