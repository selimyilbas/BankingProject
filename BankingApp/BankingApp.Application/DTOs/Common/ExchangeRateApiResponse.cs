using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BankingApp.Application.DTOs.Common
{
    /// <summary>
    /// Harici kur servisleri için basit yanıt modeli.
    /// </summary>
    public class ExchangeRateApiResponse
    {
        public string Base { get; set; } = string.Empty;
        public Dictionary<string, decimal> Rates { get; set; } = new Dictionary<string, decimal>();
    }

    /// <summary>
    /// Sunum için tek para birimi görünüm modeli.
    /// </summary>
    public class ExchangeRateDisplayDto
    {
        public string Currency { get; set; } = string.Empty;
        public string CurrencyName { get; set; } = string.Empty;
        public decimal BuyRate { get; set; }
        public decimal SellRate { get; set; }
    }

    /// <summary>
    /// Güncel kurların listelenmesi için yanıt modeli.
    /// </summary>
    public class ExchangeRatesResponseDto
    {
        public List<ExchangeRateDisplayDto> Rates { get; set; } = new List<ExchangeRateDisplayDto>();
        public DateTime LastUpdated { get; set; }
    }
}