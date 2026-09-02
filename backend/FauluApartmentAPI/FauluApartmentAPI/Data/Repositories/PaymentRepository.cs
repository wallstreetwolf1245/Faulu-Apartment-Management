using System.Linq.Expressions;
using FauluApartmentAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FauluApartmentAPI.Data.Repositories;

/// <summary>
/// Payment repository interface with specialized, landlord-scoped queries.
///
/// Every method that can return payment data to a caller now takes the
/// requesting landlord's id and filters through the ownership chain:
/// Payment -> Lease -> Unit -> Building.OwnerId (also checked via Payment.UnitId
/// directly, and via Payment.Tenant.Leases as a fallback for payments that carry
/// a TenantId but haven't been linked to a specific Lease yet).
///
/// GetByCheckoutRequestIdAsync is intentionally NOT landlord-scoped: it's used
/// by the M-Pesa STK callback handler, which runs before any authenticated
/// landlord context exists (Safaricom calls this webhook directly).
/// </summary>
public interface IPaymentRepository : IRepository<Payment>
{
    Task<IEnumerable<Payment>> GetAllForLandlordAsync(int landlordId, CancellationToken cancellationToken = default);
    Task<Payment?> GetByIdForLandlordAsync(int id, int landlordId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetPaymentsByTenantAsync(int tenantId, int landlordId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetOverduePaymentsAsync(int landlordId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetPaymentsByLeaseAsync(int leaseId, int landlordId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalOutstandingAsync(int tenantId, int landlordId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetPendingPaymentsAsync(int landlordId, CancellationToken cancellationToken = default);
    Task<Payment?> GetByCheckoutRequestIdAsync(string checkoutRequestId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Payment repository implementation
/// </summary>
public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Ownership predicate shared by every landlord-scoped query. A payment is
    /// "owned" by a landlord if it can be traced — through any of the three
    /// paths a Payment can carry — to a Building whose OwnerId matches.
    /// </summary>
    private static Expression<Func<Payment, bool>> OwnedByLandlord(int landlordId) => p =>
        (p.LeaseId != null && p.Lease != null && p.Lease.Unit != null && p.Lease.Unit.Building != null
            && p.Lease.Unit.Building.OwnerId == landlordId)
        || (p.UnitId != null && p.Unit != null && p.Unit.Building != null
            && p.Unit.Building.OwnerId == landlordId)
        || (p.TenantId != null && p.Tenant != null
            && p.Tenant.Leases.Any(l => l.Unit != null && l.Unit.Building != null && l.Unit.Building.OwnerId == landlordId));

    public async Task<IEnumerable<Payment>> GetAllForLandlordAsync(int landlordId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(OwnedByLandlord(landlordId))
            .OrderByDescending(p => p.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment?> GetByIdForLandlordAsync(int id, int landlordId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(OwnedByLandlord(landlordId))
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByTenantAsync(int tenantId, int landlordId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.TenantId == tenantId)
            .Where(OwnedByLandlord(landlordId))
            .OrderByDescending(p => p.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetOverduePaymentsAsync(int landlordId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Status == "Overdue" || (p.Status == "Pending" && p.DueDate < DateTime.UtcNow))
            .Where(OwnedByLandlord(landlordId))
            .OrderBy(p => p.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByLeaseAsync(int leaseId, int landlordId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.LeaseId == leaseId)
            .Where(OwnedByLandlord(landlordId))
            .OrderByDescending(p => p.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalOutstandingAsync(int tenantId, int landlordId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.TenantId == tenantId && (p.Status == "Pending" || p.Status == "Partial" || p.Status == "Overdue"))
            .Where(OwnedByLandlord(landlordId))
            .SumAsync(p => p.Amount - (p.PaidAmount ?? 0), cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync(int landlordId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Status == "Pending" || p.Status == "Partial")
            .Where(OwnedByLandlord(landlordId))
            .OrderBy(p => p.DueDate)
            .ToListAsync(cancellationToken);
    }

    // Not landlord-scoped on purpose — see interface doc comment above.
    public async Task<Payment?> GetByCheckoutRequestIdAsync(string checkoutRequestId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.CheckoutRequestId == checkoutRequestId, cancellationToken);
    }
}