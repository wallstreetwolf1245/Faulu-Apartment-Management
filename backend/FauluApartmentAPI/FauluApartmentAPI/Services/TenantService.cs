using FauluApartmentAPI.Data.Entities;
using FauluApartmentAPI.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FauluApartmentAPI.Services;

public interface ITenantService
{
    Task<Tenant?> GetTenantByIdAsync(int id, int landlordId, CancellationToken cancellationToken = default);
    Task<Tenant?> GetTenantByIdNumberAsync(string idNumber, int landlordId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tenant>> GetAllTenantsAsync(int landlordId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tenant>> GetActiveTenentsAsync(int landlordId, CancellationToken cancellationToken = default);
    Task<Tenant> CreateTenantAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task<Tenant> UpdateTenantAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task DeleteTenantAsync(int tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tenant>> SearchTenantsByNameAsync(string name, int landlordId, CancellationToken cancellationToken = default);
}

public class TenantService : ITenantService
{
    private readonly IRepository<Tenant> _tenantRepository;

    public TenantService(IRepository<Tenant> tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    // SCOPED: only returns the tenant if one of their leases sits on a unit
    // in a building owned by landlordId. Returns null if the tenant exists
    // but belongs to a different landlord (controller maps this to 404,
    // same as "doesn't exist" — don't leak existence across accounts).
    public async Task<Tenant?> GetTenantByIdAsync(int id, int landlordId, CancellationToken cancellationToken = default)
    {
        return await _tenantRepository.QueryableNoTracking()
            .Include(t => t.Leases.Where(l => l.Status == "Active"))
                .ThenInclude(l => l.Unit)
                    .ThenInclude(u => u.Building)
            .FirstOrDefaultAsync(t => t.Id == id
                && t.Leases.Any(l => l.Unit.Building.OwnerId == landlordId), cancellationToken);
    }

    // SCOPED
    public async Task<Tenant?> GetTenantByIdNumberAsync(string idNumber, int landlordId, CancellationToken cancellationToken = default)
    {
        return await _tenantRepository.QueryableNoTracking()
            .Include(t => t.Leases)
                .ThenInclude(l => l.Unit)
                    .ThenInclude(u => u.Building)
            .FirstOrDefaultAsync(t => t.IdentificationNumber == idNumber
                && t.Leases.Any(l => l.Unit.Building.OwnerId == landlordId), cancellationToken);
    }

    // SCOPED: this was the root cause of the cross-user tenant leak —
    // previously returned every tenant in the database with no filter.
    public async Task<IEnumerable<Tenant>> GetAllTenantsAsync(int landlordId, CancellationToken cancellationToken = default)
    {
        return await _tenantRepository.QueryableNoTracking()
            .Where(t => t.Leases.Any(l => l.Unit.Building.OwnerId == landlordId))
            .Include(t => t.Leases.Where(l => l.Status == "Active"))
                .ThenInclude(l => l.Unit)
                    .ThenInclude(u => u.Building)
            .ToListAsync(cancellationToken);
    }

    // SCOPED
    public async Task<IEnumerable<Tenant>> GetActiveTenentsAsync(int landlordId, CancellationToken cancellationToken = default)
    {
        return await _tenantRepository.QueryableNoTracking()
            .Where(t => t.Status == "Active" && t.Leases.Any(l => l.Unit.Building.OwnerId == landlordId))
            .Include(t => t.Leases.Where(l => l.Status == "Active"))
                .ThenInclude(l => l.Unit)
                    .ThenInclude(u => u.Building)
            .ToListAsync(cancellationToken);
    }

    public async Task<Tenant> CreateTenantAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        if (tenant == null)
            throw new ArgumentNullException(nameof(tenant));

        if (string.IsNullOrEmpty(tenant.FirstName) || string.IsNullOrEmpty(tenant.LastName))
            throw new ArgumentException("Tenant name is required");

        if (string.IsNullOrEmpty(tenant.IdentificationNumber))
            throw new ArgumentException("Identification number is required");

        var existingTenant = await _tenantRepository.FirstOrDefaultAsync(
            t => t.IdentificationNumber == tenant.IdentificationNumber, cancellationToken);

        if (existingTenant != null)
            throw new InvalidOperationException("Tenant with this identification number already exists");

        await _tenantRepository.AddAsync(tenant, cancellationToken);
        await _tenantRepository.SaveChangesAsync(cancellationToken);
        return tenant;
    }

    public async Task<Tenant> UpdateTenantAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        if (tenant == null)
            throw new ArgumentNullException(nameof(tenant));

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _tenantRepository.SaveChangesAsync(cancellationToken);
        return tenant;
    }

    public async Task DeleteTenantAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        await _tenantRepository.DeleteAsync(tenantId, cancellationToken);
        await _tenantRepository.SaveChangesAsync(cancellationToken);
    }

    // SCOPED: this was leaking search results across landlords too.
    public async Task<IEnumerable<Tenant>> SearchTenantsByNameAsync(string name, int landlordId, CancellationToken cancellationToken = default)
    {
        var lowerName = name.ToLower();
        return await _tenantRepository.QueryableNoTracking()
            .Where(t => (t.FirstName.ToLower().Contains(lowerName) || t.LastName.ToLower().Contains(lowerName))
                && t.Leases.Any(l => l.Unit.Building.OwnerId == landlordId))
            .Include(t => t.Leases.Where(l => l.Status == "Active"))
                .ThenInclude(l => l.Unit)
                    .ThenInclude(u => u.Building)
            .ToListAsync(cancellationToken);
    }
}