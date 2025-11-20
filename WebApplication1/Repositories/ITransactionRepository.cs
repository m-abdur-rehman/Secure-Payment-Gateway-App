using PaymentGateway.Models;

namespace PaymentGateway.Repositories.Interfaces
{
    public interface ITransactionRepository
    {
        Task AddAsync(Transaction tx);
        Task<Transaction?> FindByTransactionIdAsync(string txId);
        Task<Transaction?> FindByIdempotencyKeyAsync(string idempotencyKey);
        Task<(IEnumerable<Transaction> items, int total)> GetPagedAsync(int page, int pageSize);
    }
}
