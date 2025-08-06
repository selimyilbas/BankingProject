using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Interfaces;
using BankingApp.Infrastructure.Data;

namespace BankingApp.Infrastructure.Repositories
{
    public class ExchangeRateRepository : GenericRepository<ExchangeRateHistory>, IExchangeRateRepository
    {
        public ExchangeRateRepository(BankingDbContext context) : base(context)
        {
        }

        public async Task<decimal?> GetLatestRateAsync(string fromCurrency, string toCurrency)
        {
            var rate = await _dbSet
                .Where(r => r.FromCurrency == fromCurrency && r.ToCurrency == toCurrency)
                .OrderByDescending(r => r.CaptureDate)
                .FirstOrDefaultAsync();

            return rate?.Rate;
        }

        public async Task<ExchangeRateHistory?> GetRateByDateAsync(string fromCurrency, string toCurrency, DateTime date)
        {
            return await _dbSet
                .Where(r => r.FromCurrency == fromCurrency && 
                           r.ToCurrency == toCurrency && 
                           r.CaptureDate.Date == date.Date)
                .OrderByDescending(r => r.CaptureDate)
                .FirstOrDefaultAsync();
        }

        public async Task<ExchangeRateHistory?> GetCurrentRateAsync(string fromCurrency, string toCurrency)
        {
            return await _dbSet
                .Where(r => r.FromCurrency == fromCurrency && r.ToCurrency == toCurrency)
                .OrderByDescending(r => r.CaptureDate)
                .FirstOrDefaultAsync();
        }
    }
}
