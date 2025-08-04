using System;

namespace BankingApp.Application.DTOs.Transaction
{
    public class TransactionDto
    {
        public int TransactionId { get; set; }
        public string TransactionCode { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
