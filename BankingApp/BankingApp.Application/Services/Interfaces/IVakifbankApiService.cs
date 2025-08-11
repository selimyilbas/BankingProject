using System;
using System.Threading.Tasks;

namespace BankingApp.Application.Services.Interfaces
{
    public interface IVakifbankApiService
    {
        // Returns TRY buy/sell for given currency code on given date; null if not available
        Task<(decimal buy, decimal sell)?> GetTryRatesAsync(string currencyCode, DateTime dateUtc);

        Task<string?> GetAccessTokenAsync(string scope = "public");
    }
}


