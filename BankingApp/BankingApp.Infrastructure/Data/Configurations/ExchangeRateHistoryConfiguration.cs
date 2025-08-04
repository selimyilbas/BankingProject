using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BankingApp.Domain.Entities;

namespace BankingApp.Infrastructure.Data.Configurations
{
    public class ExchangeRateHistoryConfiguration : IEntityTypeConfiguration<ExchangeRateHistory>
    {
        public void Configure(EntityTypeBuilder<ExchangeRateHistory> builder)
        {
            builder.ToTable("ExchangeRateHistory");

            builder.HasKey(e => e.RateId);

            builder.Property(e => e.FromCurrency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(e => e.ToCurrency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(e => e.Rate)
                .HasColumnType("decimal(18, 6)")
                .IsRequired();

            builder.Property(e => e.CaptureDate)
                .IsRequired();

            builder.Property(e => e.Source)
                .IsRequired()
                .HasMaxLength(100);

            // Indexes for performance
            builder.HasIndex(e => new { e.FromCurrency, e.ToCurrency, e.CaptureDate })
                .HasDatabaseName("IX_ExchangeRate_Currencies_Date");
        }
    }
}