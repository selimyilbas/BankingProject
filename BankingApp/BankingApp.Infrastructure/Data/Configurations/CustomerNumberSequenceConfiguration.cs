using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BankingApp.Domain.Entities;

namespace BankingApp.Infrastructure.Data.Configurations
{
    /// <summary>
    /// `CustomerNumberSequence` varlığı için tablo ve veri tohumlama ayarları.
    /// </summary>
    public class CustomerNumberSequenceConfiguration : IEntityTypeConfiguration<CustomerNumberSequence>
    {
        /// <summary>
        /// Varlık-konfigürasyon eşlemesini uygular.
        /// </summary>
        public void Configure(EntityTypeBuilder<CustomerNumberSequence> builder)
        {
            builder.ToTable("CustomerNumberSequence");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.LastNumber)
                .IsRequired();

            // Seed initial data
            builder.HasData(new CustomerNumberSequence { Id = 1, LastNumber = 0 });
        }
    }
}