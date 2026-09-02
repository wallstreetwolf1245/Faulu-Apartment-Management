using FauluApartmentAPI.Data.Entities;

namespace FauluApartmentAPI.Data.Repositories;

public interface ITransactionStatusLogRepository : IRepository<TransactionStatusLog>
{
    Task<IEnumerable<TransactionStatusLog>> GetByPaymentIdAsync(int paymentId, CancellationToken cancellationToken = default);
}
