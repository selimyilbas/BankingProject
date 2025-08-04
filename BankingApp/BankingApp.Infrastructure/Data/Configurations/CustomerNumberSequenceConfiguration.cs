using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BankingApp.Domain.Entities;

namespace BankingApp.Infrastructure.Data.Configurations
{
    public class CustomerNumberSequenceConfiguration : IEntityTypeConfiguration<CustomerNumberSequence>
    {
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