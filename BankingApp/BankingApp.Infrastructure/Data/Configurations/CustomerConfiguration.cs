using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BankingApp.Domain.Entities;

namespace BankingApp.Infrastructure.Data.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");

            builder.HasKey(c => c.CustomerId);

            builder.Property(c => c.CustomerNumber)
                .IsRequired()
                .HasMaxLength(12);

            builder.Property(c => c.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.TCKN)
                .IsRequired()
                .HasMaxLength(11);

            builder.Property(c => c.Password)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.DateOfBirth)
                .IsRequired();

            builder.Property(c => c.Email)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(c => c.PhoneNumber)
                .IsRequired(false)
                .HasMaxLength(20);

            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(c => c.CreatedDate)
                .IsRequired();

            // Indexes
            builder.HasIndex(c => c.CustomerNumber)
                .IsUnique();

            builder.HasIndex(c => c.TCKN)
                .IsUnique();

            // Relationships
            builder.HasMany(c => c.Accounts)
                .WithOne(a => a.Customer)
                .HasForeignKey(a => a.CustomerId);
        }
    }
}