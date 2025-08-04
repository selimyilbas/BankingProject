// BankingApp.Domain/Entities/ExchangeRateHistory.cs
using System;

namespace BankingApp.Domain.Entities
{
    public class ExchangeRateHistory
    {
        public int RateId { get; set; } // Changed from Id to RateId
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public DateTime CaptureDate { get; set; } // Changed from RateDate to CaptureDate
        public string Source { get; set; } = string.Empty;
    }
}