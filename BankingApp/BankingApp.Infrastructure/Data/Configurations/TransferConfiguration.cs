using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BankingApp.Domain.Entities;

namespace BankingApp.Infrastructure.Data.Configurations
{
    /// <summary>
    /// `Transfer` varlığı için tablo, indeks ve ilişki ayarları.
    /// </summary>
    public class TransferConfiguration : IEntityTypeConfiguration<Transfer>
    {
        /// <summary>
        /// Varlık-konfigürasyon eşlemesini uygular.
        /// </summary>
        public void Configure(EntityTypeBuilder<Transfer> builder)
        {
            builder.ToTable("Transfers");

            builder.HasKey(t => t.TransferId);

            builder.Property(t => t.TransferCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(t => t.Amount)
                .HasColumnType("decimal(18, 2)")
                .IsRequired();

            builder.Property(t => t.FromCurrency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(t => t.ToCurrency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(t => t.ExchangeRate)
                .HasColumnType("decimal(18, 6)");

            builder.Property(t => t.ConvertedAmount)
                .HasColumnType("decimal(18, 2)");

            builder.Property(t => t.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("PENDING");

            builder.Property(t => t.Description)
                .HasMaxLength(500);

            builder.Property(t => t.TransferDate)
                .IsRequired();

            // Relationships
            builder.HasOne(t => t.FromAccount)
                .WithMany(a => a.TransfersFrom)
                .HasForeignKey(t => t.FromAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.ToAccount)
                .WithMany(a => a.TransfersTo)
                .HasForeignKey(t => t.ToAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index on TransferCode for uniqueness
            builder.HasIndex(t => t.TransferCode)
                .IsUnique();

            // Index for performance
            builder.HasIndex(t => t.TransferDate);
            builder.HasIndex(t => new { t.FromAccountId, t.TransferDate });
            builder.HasIndex(t => new { t.ToAccountId, t.TransferDate });

            // Ignore navigation-only properties to avoid shadow fk columns
            builder.Ignore(t => t.FromTransaction);
            builder.Ignore(t => t.ToTransaction);
        }
    }
}