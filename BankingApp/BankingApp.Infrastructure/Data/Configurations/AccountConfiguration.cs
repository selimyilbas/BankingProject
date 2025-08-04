using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BankingApp.Domain.Entities;

namespace BankingApp.Infrastructure.Data.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("Accounts");

            builder.HasKey(a => a.AccountId);

            builder.Property(a => a.AccountNumber)
                .IsRequired()
                .HasMaxLength(12);

            builder.Property(a => a.Currency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(a => a.Balance)
                .HasColumnType("decimal(18, 2)")
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(a => a.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(a => a.CreatedDate)
                .IsRequired();

            // Indexes
            builder.HasIndex(a => a.AccountNumber)
                .IsUnique();

            builder.HasIndex(a => new { a.CustomerId, a.Currency });

            // Relationships
            builder.HasOne(a => a.Customer)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.CustomerId);

            builder.HasMany(a => a.Transactions)
                .WithOne(t => t.Account)
                .HasForeignKey(t => t.AccountId);
        }
    }
}