using System;

namespace BankingApp.Application.DTOs.Transfer
{
    /// <summary>
    /// Transfer detaylarını temsil eden DTO.
    /// </summary>
    public class TransferDto
    {
        public int TransferId { get; set; }
        public string TransferCode { get; set; } = string.Empty;
        public string FromAccountNumber { get; set; } = string.Empty;
        public string ToAccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal? ExchangeRate { get; set; }
        public decimal ConvertedAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime TransferDate { get; set; }
    }
}
