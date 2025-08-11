using System.Threading.Tasks;
using BankingApp.Application.DTOs.Common;

namespace BankingApp.Application.Services.Interfaces
{
    /// <summary>
    /// Döviz kuru işlemleri için servis sözleşmesi.
    /// </summary>
    public interface IExchangeRateService
    {
        /// <summary>
        /// İki para birimi arasındaki kuru döner.
        /// </summary>
        Task<ApiResponse<decimal>> GetExchangeRateAsync(string fromCurrency, string toCurrency, bool skipCache = false);
        /// <summary>
        /// Tutarı hedef para birimine çevirir.
        /// </summary>
        Task<ApiResponse<decimal>> ConvertAmountAsync(decimal amount, string fromCurrency, string toCurrency);
        /// <summary>
        /// Kur bilgilerini harici kaynaktan günceller.
        /// </summary>
        Task UpdateExchangeRatesAsync();
        /// <summary>
        /// Sunum için seçili kurların güncel değerlerini döner.
        /// </summary>
        Task<ApiResponse<ExchangeRatesResponseDto>> GetCurrentExchangeRatesAsync(bool skipCache = false);
    }
}
