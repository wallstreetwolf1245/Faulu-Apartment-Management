using FauluApartmentAPI.Data.Entities;
using FauluApartmentAPI.Data.Repositories;
using FauluApartmentAPI.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace FauluApartmentAPI.Services;

public interface IReversalService
{
    Task<PaymentReversal> CreateReversalAsync(int paymentId, string reason, CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentReversal>> GetReversalsByBuildingAsync(int buildingId, CancellationToken cancellationToken = default);
    Task<PaymentReversal?> GetReversalByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PaymentReversal> UpdateReversalStatusAsync(int id, string status, CancellationToken cancellationToken = default);
}

public class ReversalService : IReversalService
{
    private readonly IRepository<PaymentReversal> _reversalRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IRepository<Tenant> _tenantRepository;

    public ReversalService(IRepository<PaymentReversal> reversalRepository,
        IPaymentRepository paymentRepository,
        IRepository<Tenant> tenantRepository)
    {
        _reversalRepository = reversalRepository;
        _paymentRepository = paymentRepository;
        _tenantRepository = tenantRepository;
    }

    public async Task<PaymentReversal> CreateReversalAsync(int paymentId, string reason, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null)
            throw new InvalidOperationException("Payment not found");

        if (payment.PaidAmount == null || payment.PaidAmount <= 0)
            throw new InvalidOperationException("Only payments with a paid amount can be reversed");

        var tenant = payment.TenantId.HasValue ? await _tenantRepository.GetByIdAsync(payment.TenantId.Value, cancellationToken) : null;

        var reversal = new PaymentReversal
        {
            PaymentId = payment.Id,
            TenantId = payment.TenantId ?? 0,
            BuildingId = payment.Unit != null ? payment.Unit.BuildingId : null,
            Amount = payment.PaidAmount ?? 0m,
            Reason = reason,
            Status = "Pending",
            RequestedAt = DateTime.UtcNow,
            PaymentReferenceId = payment.TransactionReference ?? payment.MpesaReceiptNumber
        };

        await _reversalRepository.AddAsync(reversal, cancellationToken);
        await _reversalRepository.SaveChangesAsync(cancellationToken);
        return reversal;
    }

    public async Task<IEnumerable<PaymentReversal>> GetReversalsByBuildingAsync(int buildingId, CancellationToken cancellationToken = default)
    {
        var query = _reversalRepository.QueryableNoTracking()
            .Where(r => r.BuildingId == buildingId)
            .OrderByDescending(r => r.RequestedAt);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<PaymentReversal?> GetReversalByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _reversalRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<PaymentReversal> UpdateReversalStatusAsync(int id, string status, CancellationToken cancellationToken = default)
    {
        var reversal = await _reversalRepository.GetByIdAsync(id, cancellationToken);
        if (reversal == null)
            throw new InvalidOperationException("Reversal not found");

        if (reversal.Status != "Pending")
            throw new InvalidOperationException("Only pending reversals can be processed");

        if (status != "Approved" && status != "Rejected")
            throw new ArgumentException("Invalid status");

        reversal.Status = status;
        reversal.ProcessedAt = DateTime.UtcNow;

        if (status == "Approved")
        {
            var payment = await _paymentRepository.GetByIdAsync(reversal.PaymentId, cancellationToken);
            if (payment == null)
                throw new InvalidOperationException("Linked payment not found");

            payment.PaidAmount = (payment.PaidAmount ?? 0) - reversal.Amount;
            if (payment.PaidAmount <= 0)
            {
                payment.PaidAmount = 0;
                payment.Status = "Pending";
                payment.PaidDate = null;
            }
            else if (payment.PaidAmount < payment.Amount)
            {
                payment.Status = "Partial";
            }

            await _paymentRepository.UpdateAsync(payment, cancellationToken);
        }

        await _reversalRepository.UpdateAsync(reversal, cancellationToken);
        await _reversalRepository.SaveChangesAsync(cancellationToken);
        return reversal;
    }
}