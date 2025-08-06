using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BankingApp.Application.DTOs.Common
{
    public class ExchangeRateApiResponse
    {
        public string Base { get; set; } = string.Empty;
        public Dictionary<string, decimal> Rates { get; set; } = new Dictionary<string, decimal>();
    }

    public class ExchangeRateDisplayDto
    {
        public string Currency { get; set; } = string.Empty;
        public string CurrencyName { get; set; } = string.Empty;
        public decimal BuyRate { get; set; }
        public decimal SellRate { get; set; }
    }

    public class ExchangeRatesResponseDto
    {
        public List<ExchangeRateDisplayDto> Rates { get; set; } = new List<ExchangeRateDisplayDto>();
        public DateTime LastUpdated { get; set; }
    }
}