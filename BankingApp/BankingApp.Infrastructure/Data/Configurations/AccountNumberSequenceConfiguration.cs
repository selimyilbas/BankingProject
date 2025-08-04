using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BankingApp.Domain.Entities;

namespace BankingApp.Infrastructure.Data.Configurations
{
    public class AccountNumberSequenceConfiguration : IEntityTypeConfiguration<AccountNumberSequence>
    {
        public void Configure(EntityTypeBuilder<AccountNumberSequence> builder)
        {
            builder.ToTable("AccountNumberSequence");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Currency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(a => a.LastNumber)
                .IsRequired();

            // Index for currency lookup
            builder.HasIndex(a => a.Currency)
                .IsUnique();

            // Seed initial data
            builder.HasData(
                new AccountNumberSequence { Id = 1, Currency = "TL", LastNumber = 0 },
                new AccountNumberSequence { Id = 2, Currency = "EUR", LastNumber = 0 },
                new AccountNumberSequence { Id = 3, Currency = "USD", LastNumber = 0 }
            );
        }
    }
}