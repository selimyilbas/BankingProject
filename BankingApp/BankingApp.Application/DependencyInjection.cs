// BankingApp.Application/DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;
using BankingApp.Application.Services.Interfaces;
using BankingApp.Application.Services.Implementations;
using BankingApp.Application.Mappings;

namespace BankingApp.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register AutoMapper
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            // Register services
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<ITransferService, TransferService>();
            services.AddScoped<IExchangeRateService, ExchangeRateService>();

            return services;
        }
    }
}