using Microsoft.EntityFrameworkCore;
using PaymentGateway.Data;
using PaymentGateway.Repositories.Interfaces;

namespace WebApplication1.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _db;

        public TransactionRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Transaction tx)
        {
            await _db.Transactions.AddAsync(tx);
            await _db.SaveChangesAsync();
        }

        public async Task<Transaction?> FindByTransactionIdAsync(string txId)
        {
            return await _db.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == txId && !t.IsDeleted);
        }

        public async Task<Transaction?> FindByIdempotencyKeyAsync(string idempotencyKey)
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey))
                return null;

            return await _db.Transactions
                .FirstOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey && !t.IsDeleted);
        }

        public async Task<(IEnumerable<Transaction> items, int total)> GetPagedAsync(int page, int pageSize)
        {
            var query = _db.Transactions
                .Where(t => !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt);

            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
