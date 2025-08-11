using System;
using System.Threading.Tasks;

namespace BankingApp.Application.Services.Interfaces
{
    /// <summary>
    /// VakıfBank entegrasyonu için dış servis sözleşmesi.
    /// </summary>
    public interface IVakifbankApiService
    {
        /// <summary>
        /// Verilen tarihteki TRY alış/satış oranlarını döner; bulunamazsa null.
        /// </summary>
        Task<(decimal buy, decimal sell)?> GetTryRatesAsync(string currencyCode, DateTime dateUtc);

        /// <summary>
        /// API erişim belirteci alır (deneysel).
        /// </summary>
        Task<string?> GetAccessTokenAsync(string scope = "public");
    }
}


