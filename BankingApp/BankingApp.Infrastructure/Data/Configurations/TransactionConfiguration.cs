using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BankingApp.Domain.Entities;

namespace BankingApp.Infrastructure.Data.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");

            builder.HasKey(t => t.TransactionId);

            builder.Property(t => t.TransactionCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(t => t.TransactionType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(t => t.Amount)
                .HasColumnType("decimal(18, 2)")
                .IsRequired();

            builder.Property(t => t.Currency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(t => t.ExchangeRate)
                .HasColumnType("decimal(18, 6)")
                .IsRequired();

            builder.Property(t => t.Description)
                .HasMaxLength(500);

            builder.Property(t => t.TransactionDate)
                .IsRequired();

            builder.Property(t => t.CreatedDate)
                .IsRequired();

            // Indexes
            builder.HasIndex(t => t.TransactionCode)
                .IsUnique();

            builder.HasIndex(t => new { t.AccountId, t.TransactionDate });

            // Relationships
            builder.HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId);
        }
    }
}