using Microsoft.EntityFrameworkCore;
using BankingApp.Domain.Entities;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace BankingApp.Infrastructure.Data
{
    public class BankingDbContext : DbContext
    {
        public BankingDbContext(DbContextOptions<BankingDbContext> options)
            : base(options)
        {
        }

        // DbSets
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

            // Apply configurations from assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

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
