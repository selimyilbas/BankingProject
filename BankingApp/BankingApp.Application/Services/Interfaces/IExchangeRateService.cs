using System.Threading.Tasks;
using BankingApp.Application.DTOs.Common;

namespace BankingApp.Application.Services.Interfaces
{
    public interface IExchangeRateService
    {
        Task<ApiResponse<decimal>> GetExchangeRateAsync(string fromCurrency, string toCurrency);
        Task<ApiResponse<decimal>> ConvertAmountAsync(decimal amount, string fromCurrency, string toCurrency);
        Task UpdateExchangeRatesAsync();
        Task<ApiResponse<ExchangeRatesResponseDto>> GetCurrentExchangeRatesAsync();
    }
}
