using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using BankingApp.Domain.Interfaces;
using BankingApp.Infrastructure.Data;
using BankingApp.Infrastructure.Repositories;

namespace BankingApp.Infrastructure
{
    /// <summary>
    /// Infrastructure katmanı DI kayıtlarını içeren yardımcı sınıf.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Infrastructure bağımlılıklarını ve DbContext'i kaydeder.
        /// </summary>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext kaydı
            services.AddDbContext<BankingDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Repository kayıtları
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<ITransferRepository, TransferRepository>();
            services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
            
            // Unit of Work kaydı
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}