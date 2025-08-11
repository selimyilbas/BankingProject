// BankingApp.Domain/Entities/Transaction.js
using System;

namespace BankingApp.Domain.Entities
{
    /// <summary>
    /// Hesap hareketi (transaction) varlığı.
    /// </summary>
    public class Transaction
    {
        public int TransactionId { get; set; }
        public string TransactionCode { get; set; } = string.Empty;
        public int AccountId { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// İşlemin ait olduğu hesap.
        /// </summary>
        public virtual Account Account { get; set; } = null!;
    }
}
