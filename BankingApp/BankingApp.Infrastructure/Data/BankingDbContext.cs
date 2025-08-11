using Microsoft.EntityFrameworkCore;
using BankingApp.Domain.Entities;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace BankingApp.Infrastructure.Data
{
    /// <summary>
    /// Uygulamanın EF Core DbContext'i. DbSet'ler ve model konfigurasyonlarını içerir.
    /// </summary>
    public class BankingDbContext : DbContext
    {
        /// <summary>
        /// DbContext örneğini konfigürasyonla başlatır.
        /// </summary>
        public BankingDbContext(DbContextOptions<BankingDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// DbSet tanımları.
        /// </summary>
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<CustomerNumberSequence> CustomerNumberSequences { get; set; }
        public DbSet<AccountNumberSequence> AccountNumberSequences { get; set; }
        public DbSet<ExchangeRateHistory> ExchangeRateHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mevcut assembly'deki tüm IEntityTypeConfiguration sınıflarını uygular
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// SaveChangesAsync sırasında bazı varlıkların UpdatedDate alanını otomatik set eder.
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Modified)
                {
                    if (entry.Entity is Customer customer)
                    {
                        customer.UpdatedDate = DateTime.UtcNow;
                    }
                    else if (entry.Entity is Account account)
                    {
                        account.UpdatedDate = DateTime.UtcNow;
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
