using System;
using System.Threading.Tasks;
using BankingApp.Domain.Entities;

namespace BankingApp.Domain.Interfaces
{
    /// <summary>
    /// Döviz kuru geçmişi için veri erişim işlemleri.
    /// </summary>
    public interface IExchangeRateRepository : IGenericRepository<ExchangeRateHistory>
    {
        /// <summary>
        /// En güncel kuru getirir.
        /// </summary>
        Task<decimal?> GetLatestRateAsync(string fromCurrency, string toCurrency);
        /// <summary>
        /// Belirli tarihteki kuru getirir.
        /// </summary>
        Task<ExchangeRateHistory?> GetRateByDateAsync(string fromCurrency, string toCurrency, DateTime date);
        /// <summary>
        /// Şu an geçerli kuru getirir.
        /// </summary>
        Task<ExchangeRateHistory?> GetCurrentRateAsync(string fromCurrency, string toCurrency);
    }
}
