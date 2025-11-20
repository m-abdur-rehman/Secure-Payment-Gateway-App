using Microsoft.EntityFrameworkCore;

namespace PaymentGateway.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Transaction>()
                .HasIndex(t => t.TransactionId)
                .IsUnique();

            builder.Entity<Transaction>()
                .HasIndex(t => t.MobileNumber);

            // Index for idempotency key lookup (unique constraint)
            builder.Entity<Transaction>()
                .HasIndex(t => t.IdempotencyKey)
                .IsUnique()
                .HasFilter("[IdempotencyKey] IS NOT NULL");
        }
    }
}
