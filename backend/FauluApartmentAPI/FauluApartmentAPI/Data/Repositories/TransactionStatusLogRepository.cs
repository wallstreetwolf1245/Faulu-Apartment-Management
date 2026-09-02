using FauluApartmentAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FauluApartmentAPI.Data.Repositories;

public class TransactionStatusLogRepository : Repository<TransactionStatusLog>, ITransactionStatusLogRepository
{
    public TransactionStatusLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TransactionStatusLog>> GetByPaymentIdAsync(int paymentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(t => t.PaymentId == paymentId).OrderByDescending(t => t.ReceivedAt).ToListAsync(cancellationToken);
    }
}
