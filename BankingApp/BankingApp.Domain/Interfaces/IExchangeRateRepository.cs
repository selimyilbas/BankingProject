using System;
using System.Threading.Tasks;
using BankingApp.Domain.Entities;

namespace BankingApp.Domain.Interfaces
{
    public interface IExchangeRateRepository : IGenericRepository<ExchangeRateHistory>
    {
        Task<decimal?> GetLatestRateAsync(string fromCurrency, string toCurrency);
        Task<ExchangeRateHistory?> GetRateByDateAsync(string fromCurrency, string toCurrency, DateTime date);
        Task<ExchangeRateHistory?> GetCurrentRateAsync(string fromCurrency, string toCurrency);
    }
}
