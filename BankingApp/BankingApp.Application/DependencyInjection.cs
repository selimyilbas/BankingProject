// BankingApp.Application/DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using BankingApp.Application.Services.Interfaces;
using BankingApp.Application.Services.Implementations;
using BankingApp.Application.Mappings;
using Microsoft.Extensions.Caching.Memory;

namespace BankingApp.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ITransferService, TransferService>();
            services.AddScoped<IExchangeRateService, ExchangeRateService>();
            services.AddScoped<ITransactionService, TransactionService>();
            
            // Add HttpClient for ExchangeRateService
            services.AddHttpClient<ExchangeRateService>();
            services.AddMemoryCache();
            
            return services;
        }
    }
}