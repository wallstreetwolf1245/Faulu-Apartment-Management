using FauluApartmentAPI.Data.Entities;
using FauluApartmentAPI.Data.Repositories;

namespace FauluApartmentAPI.Services;

/// <summary>
/// Lease service interface for lease operations
/// </summary>
public interface ILeaseService
{
    Task<Lease?> GetLeaseByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lease>> GetLeasesByTenantAsync(int tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lease>> GetLeasesByUnitAsync(int unitId, CancellationToken cancellationToken = default);
    Task<Lease?> GetCurrentLeaseByUnitAsync(int unitId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lease>> GetExpiringLeasesAsync(int daysUntilExpiry, CancellationToken cancellationToken = default);
    Task<Lease> CreateLeaseAsync(Lease lease, CancellationToken cancellationToken = default);
    Task<Lease> UpdateLeaseAsync(Lease lease, CancellationToken cancellationToken = default);
    Task<Lease> RenewLeaseAsync(int leaseId, DateTime newEndDate, CancellationToken cancellationToken = default);
    Task TerminateLeaseAsync(int leaseId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Lease service implementation
/// </summary>
public class LeaseService : ILeaseService
{
    private readonly ILeaseRepository _leaseRepository;
    private readonly IUnitRepository _unitRepository;

    public LeaseService(ILeaseRepository leaseRepository, IUnitRepository unitRepository)
    {
        _leaseRepository = leaseRepository;
        _unitRepository = unitRepository;
    }

    public async Task<Lease?> GetLeaseByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _leaseRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Lease>> GetLeasesByTenantAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        return await _leaseRepository.GetLeasesByTenantAsync(tenantId, cancellationToken);
    }

    public async Task<IEnumerable<Lease>> GetLeasesByUnitAsync(int unitId, CancellationToken cancellationToken = default)
    {
        return await _leaseRepository.GetActiveLeasesByUnitAsync(unitId, cancellationToken);
    }

    public async Task<Lease?> GetCurrentLeaseByUnitAsync(int unitId, CancellationToken cancellationToken = default)
    {
        return await _leaseRepository.GetCurrentLeaseByUnitAsync(unitId, cancellationToken);
    }

    public async Task<IEnumerable<Lease>> GetExpiringLeasesAsync(int daysUntilExpiry, CancellationToken cancellationToken = default)
    {
        return await _leaseRepository.GetExpiringLeasesAsync(daysUntilExpiry, cancellationToken);
    }

    public async Task<Lease> CreateLeaseAsync(Lease lease, CancellationToken cancellationToken = default)
    {
        if (lease == null)
            throw new ArgumentNullException(nameof(lease));

        if (lease.StartDate >= lease.EndDate)
            throw new ArgumentException("Lease start date must be before end date");

        var currentLease = await _leaseRepository.GetCurrentLeaseByUnitAsync(lease.UnitId, cancellationToken);
        if (currentLease != null)
            throw new InvalidOperationException("Unit already has an active lease");

        await _leaseRepository.AddAsync(lease, cancellationToken);
        await _leaseRepository.SaveChangesAsync(cancellationToken);

        // Update unit status to Occupied
        var unit = await _unitRepository.GetByIdAsync(lease.UnitId, cancellationToken);
        if (unit != null)
        {
            unit.Status = "Occupied";
            await _unitRepository.UpdateAsync(unit, cancellationToken);
            await _unitRepository.SaveChangesAsync(cancellationToken);
        }

        return lease;
    }

    public async Task<Lease> UpdateLeaseAsync(Lease lease, CancellationToken cancellationToken = default)
    {
        if (lease == null)
            throw new ArgumentNullException(nameof(lease));

        await _leaseRepository.UpdateAsync(lease, cancellationToken);
        await _leaseRepository.SaveChangesAsync(cancellationToken);
        return lease;
    }

    public async Task<Lease> RenewLeaseAsync(int leaseId, DateTime newEndDate, CancellationToken cancellationToken = default)
    {
        var lease = await _leaseRepository.GetByIdAsync(leaseId, cancellationToken);
        if (lease == null)
            throw new InvalidOperationException("Lease not found");

        if (newEndDate <= DateTime.UtcNow)
            throw new ArgumentException("New end date must be in the future");

        lease.EndDate = newEndDate;
        lease.Status = "Renewed";
        await _leaseRepository.UpdateAsync(lease, cancellationToken);
        await _leaseRepository.SaveChangesAsync(cancellationToken);

        return lease;
    }

    public async Task TerminateLeaseAsync(int leaseId, CancellationToken cancellationToken = default)
    {
        var lease = await _leaseRepository.GetByIdAsync(leaseId, cancellationToken);
        if (lease == null)
            throw new InvalidOperationException("Lease not found");

        lease.Status = "Terminated";
        lease.EndDate = DateTime.UtcNow;
        await _leaseRepository.UpdateAsync(lease, cancellationToken);
        await _leaseRepository.SaveChangesAsync(cancellationToken);

        // Update unit status to Vacant
        var unit = await _unitRepository.GetByIdAsync(lease.UnitId, cancellationToken);
        if (unit != null)
        {
            unit.Status = "Vacant";
            await _unitRepository.UpdateAsync(unit, cancellationToken);
            await _unitRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
