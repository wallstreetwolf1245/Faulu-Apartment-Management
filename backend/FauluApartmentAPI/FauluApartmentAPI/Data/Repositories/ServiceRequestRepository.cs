using FauluApartmentAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FauluApartmentAPI.Data.Repositories;

/// <summary>
/// ServiceRequest repository interface with specialized queries
/// </summary>
public interface IServiceRequestRepository : IRepository<ServiceRequest>
{
    Task<IEnumerable<ServiceRequest>> GetRequestsByTenantAsync(int tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceRequest>> GetOpenRequestsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceRequest>> GetRequestsByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceRequest>> GetRequestsAssignedToAsync(int managerId, CancellationToken cancellationToken = default);
    Task<int> GetOpenRequestsCountAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// ServiceRequest repository implementation
/// </summary>
public class ServiceRequestRepository : Repository<ServiceRequest>, IServiceRequestRepository
{
    public ServiceRequestRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ServiceRequest>> GetRequestsByTenantAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.RequestDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ServiceRequest>> GetOpenRequestsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.Status == "Open" || s.Status == "InProgress")
            .OrderByDescending(s => s.Priority == "Urgent" ? 0 : 1)
            .ThenByDescending(s => s.RequestDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ServiceRequest>> GetRequestsByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.Status == status)
            .OrderByDescending(s => s.RequestDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ServiceRequest>> GetRequestsAssignedToAsync(int managerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.AssignedManagerId == managerId)
            .OrderByDescending(s => s.RequestDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetOpenRequestsCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(s => s.Status == "Open" || s.Status == "InProgress", cancellationToken);
    }
}
