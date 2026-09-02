using FauluApartmentAPI.Data.Entities;
using FauluApartmentAPI.Data.Repositories;

namespace FauluApartmentAPI.Services;

public interface IPaymentService
{
    Task<IEnumerable<Payment>> GetAllPaymentsAsync(int landlordId, CancellationToken cancellationToken = default);
    Task<Payment?> GetPaymentByIdAsync(int id, int landlordId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetPaymentsByTenantAsync(int tenantId, int landlordId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetPaymentsByLeaseAsync(int leaseId, int landlordId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetOverduePaymentsAsync(int landlordId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetPendingPaymentsAsync(int landlordId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalOutstandingAsync(int tenantId, int landlordId, CancellationToken cancellationToken = default);
    Task<Payment> CreatePaymentAsync(Payment payment, int landlordId, CancellationToken cancellationToken = default);
    Task<Payment> RecordFullPaymentAsync(Payment payment, string? rentalPeriod, int landlordId, CancellationToken cancellationToken = default);
    Task<Payment> RecordPaymentAsync(int paymentId, decimal amount, string paymentMethod, string? transactionRef, int landlordId, CancellationToken cancellationToken = default);
    Task<Payment> UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default);
    Task DeletePaymentAsync(int paymentId, CancellationToken cancellationToken = default);
}

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IRepository<Tenant> _tenantRepository;
    private readonly IRepository<Lease> _leaseRepository;

    public PaymentService(IPaymentRepository paymentRepository,
        IRepository<Tenant> tenantRepository,
        IRepository<Lease> leaseRepository)
    {
        _paymentRepository = paymentRepository;
        _tenantRepository = tenantRepository;
        _leaseRepository = leaseRepository;
    }

    public async Task<IEnumerable<Payment>> GetAllPaymentsAsync(int landlordId, CancellationToken cancellationToken = default)
    {
        return await _paymentRepository.GetAllForLandlordAsync(landlordId, cancellationToken);
    }

    public async Task<Payment?> GetPaymentByIdAsync(int id, int landlordId, CancellationToken cancellationToken = default)
    {
        return await _paymentRepository.GetByIdForLandlordAsync(id, landlordId, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByTenantAsync(int tenantId, int landlordId, CancellationToken cancellationToken = default)
    {
        return await _paymentRepository.GetPaymentsByTenantAsync(tenantId, landlordId, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByLeaseAsync(int leaseId, int landlordId, CancellationToken cancellationToken = default)
    {
        return await _paymentRepository.GetPaymentsByLeaseAsync(leaseId, landlordId, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetOverduePaymentsAsync(int landlordId, CancellationToken cancellationToken = default)
    {
        return await _paymentRepository.GetOverduePaymentsAsync(landlordId, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync(int landlordId, CancellationToken cancellationToken = default)
    {
        return await _paymentRepository.GetPendingPaymentsAsync(landlordId, cancellationToken);
    }

    public async Task<decimal> GetTotalOutstandingAsync(int tenantId, int landlordId, CancellationToken cancellationToken = default)
    {
        return await _paymentRepository.GetTotalOutstandingAsync(tenantId, landlordId, cancellationToken);
    }

    public async Task<Payment> CreatePaymentAsync(Payment payment, int landlordId, CancellationToken cancellationToken = default)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        if (payment.Amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero");

        if (payment.DueDate == default)
            throw new ArgumentException("Due date is required");

        // Only validate tenant/lease for manual payments — M-Pesa C2B payments
        // arrive without a known tenant or lease until matched
        if (payment.PaymentSource != "Mpesa_C2B")
        {
            await AttachTenantAndLeaseAsync(payment, cancellationToken);

            // TODO: verify the attached tenant/lease actually belongs to `landlordId`
            // (i.e. tenant's unit's building.OwnerId == landlordId) before allowing the
            // payment to be created. Needs a queryable path into ITenantRepository /
            // ILeaseRepository that isn't available from the generic IRepository<T>
            // interface shown so far — send that file and this closes the last gap.
        }

        if (payment.DueDate < DateTime.UtcNow && payment.Status == "Pending")
            payment.Status = "Overdue";

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _paymentRepository.SaveChangesAsync(cancellationToken);
        return payment;
    }

    /// <summary>
    /// Logs a payment that already happened, in full — cash handed over, a bank
    /// transfer already confirmed. No invoice/due-date cycle; the record is
    /// created already marked Paid.
    /// </summary>
    public async Task<Payment> RecordFullPaymentAsync(Payment payment, string? rentalPeriod, int landlordId, CancellationToken cancellationToken = default)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        if (payment.Amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero");

        await AttachTenantAndLeaseAsync(payment, cancellationToken);

        // TODO: same ownership check as CreatePaymentAsync — see note above.

        payment.PaidAmount = payment.Amount;
        payment.Status = "Paid";
        payment.PaymentSource = "Manual";

        if (payment.PaidDate == null || payment.PaidDate.Value == default)
            payment.PaidDate = DateTime.UtcNow;

        if (payment.DueDate == default)
            payment.DueDate = payment.PaidDate.Value;

        if (!string.IsNullOrWhiteSpace(rentalPeriod))
        {
            payment.Notes = string.IsNullOrWhiteSpace(payment.Notes)
                ? $"Period: {rentalPeriod}"
                : $"{payment.Notes} | Period: {rentalPeriod}";
        }

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _paymentRepository.SaveChangesAsync(cancellationToken);
        return payment;
    }

    public async Task<Payment> RecordPaymentAsync(int paymentId, decimal amount, string paymentMethod, string? transactionRef, int landlordId, CancellationToken cancellationToken = default)
    {
        // SCOPED: previously fetched by bare id — any authenticated landlord could
        // record a payment against any tenant's payment in the system.
        var payment = await _paymentRepository.GetByIdForLandlordAsync(paymentId, landlordId, cancellationToken);
        if (payment == null)
            throw new InvalidOperationException("Payment not found");

        if (amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero");

        payment.PaidAmount = (payment.PaidAmount ?? 0) + amount;
        payment.PaymentMethod = paymentMethod;
        payment.TransactionReference = transactionRef;
        payment.PaidDate = DateTime.UtcNow;

        if (payment.PaidAmount >= payment.Amount)
            payment.Status = "Paid";
        else if (payment.PaidAmount > 0)
            payment.Status = "Partial";

        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        await _paymentRepository.SaveChangesAsync(cancellationToken);
        return payment;
    }

    public async Task<Payment> UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        // Not landlord-scoped here by design, matching TenantsController.UpdateTenant's
        // pattern: the controller fetches the payment via GetPaymentByIdAsync(id, landlordId)
        // first (which 404s if not owned), then passes the already-verified entity in here.
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        await _paymentRepository.SaveChangesAsync(cancellationToken);
        return payment;
    }

    public async Task DeletePaymentAsync(int paymentId, CancellationToken cancellationToken = default)
    {
        // Same pattern as UpdatePaymentAsync — the controller must verify ownership
        // via GetPaymentByIdAsync(id, landlordId) before calling this.
        await _paymentRepository.DeleteAsync(paymentId, cancellationToken);
        await _paymentRepository.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Shared tenant/lease validation used by both CreatePaymentAsync and
    /// RecordFullPaymentAsync, so the two flows can't drift out of sync.
    /// </summary>
    private async Task AttachTenantAndLeaseAsync(Payment payment, CancellationToken cancellationToken)
    {
        if (!payment.TenantId.HasValue)
            throw new ArgumentException("Tenant is required for manual payments");

        var tenant = await _tenantRepository.GetByIdAsync(payment.TenantId.Value, cancellationToken);
        if (tenant == null)
            throw new ArgumentException("Tenant not found");

        payment.Tenant = tenant;

        if (payment.LeaseId.HasValue)
        {
            var lease = await _leaseRepository.GetByIdAsync(payment.LeaseId.Value, cancellationToken);
            if (lease == null)
                throw new ArgumentException("Lease not found");

            if (lease.TenantId != payment.TenantId.Value)
                throw new ArgumentException("Lease does not belong to the specified tenant");

            payment.Lease = lease;
        }
    }
}