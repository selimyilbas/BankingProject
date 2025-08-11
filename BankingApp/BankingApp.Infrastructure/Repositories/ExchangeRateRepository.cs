using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Interfaces;
using BankingApp.Infrastructure.Data;

namespace BankingApp.Infrastructure.Repositories
{
    /// <summary>
    /// Döviz kuru geçmişine yönelik veri erişim işlemleri.
    /// </summary>
    public class ExchangeRateRepository : GenericRepository<ExchangeRateHistory>, IExchangeRateRepository
    {
        /// <summary>
        /// Repository örneğini başlatır.
        /// </summary>
        public ExchangeRateRepository(BankingDbContext context) : base(context)
        {
        }

        /// <summary>
        /// En güncel kuru döner.
        /// </summary>
        public async Task<decimal?> GetLatestRateAsync(string fromCurrency, string toCurrency)
        {
            var rate = await _dbSet
                .Where(r => r.FromCurrency == fromCurrency && r.ToCurrency == toCurrency)
                .OrderByDescending(r => r.CaptureDate)
                .FirstOrDefaultAsync();

            return rate?.Rate;
        }

        /// <summary>
        /// Belirli tarihteki kuru döner.
        /// </summary>
        public async Task<ExchangeRateHistory?> GetRateByDateAsync(string fromCurrency, string toCurrency, DateTime date)
        {
            return await _dbSet
                .Where(r => r.FromCurrency == fromCurrency && 
                           r.ToCurrency == toCurrency && 
                           r.CaptureDate.Date == date.Date)
                .OrderByDescending(r => r.CaptureDate)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// En son geçerli kuru döner.
        /// </summary>
        public async Task<ExchangeRateHistory?> GetCurrentRateAsync(string fromCurrency, string toCurrency)
        {
            return await _dbSet
                .Where(r => r.FromCurrency == fromCurrency && r.ToCurrency == toCurrency)
                .OrderByDescending(r => r.CaptureDate)
                .FirstOrDefaultAsync();
        }
    }
}
