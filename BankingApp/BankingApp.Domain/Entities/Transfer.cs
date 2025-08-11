// BankingApp.Domain/Entities/Transfer.cs
using System;
using System.Collections.Generic;

namespace BankingApp.Domain.Entities
{
    /// <summary>
    /// Para transferi varlığı: kaynak/hedef hesaplar ve tutarlar.
    /// </summary>
    public class Transfer
    {
        public int TransferId { get; set; }
        public string TransferCode { get; set; } = string.Empty; // Added this property
        public int FromAccountId { get; set; }
        public int ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal? ExchangeRate { get; set; }
        public decimal? ConvertedAmount { get; set; }
        public string Status { get; set; } = string.Empty; // PENDING, COMPLETED, FAILED, CANCELLED
        public string? Description { get; set; }
        public DateTime TransferDate { get; set; } // Renamed from CreatedDate
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// Kaynak hesap.
        /// </summary>
        public virtual Account FromAccount { get; set; } = null!;
        /// <summary>
        /// Hedef hesap.
        /// </summary>
        public virtual Account ToAccount { get; set; } = null!;
        /// <summary>
        /// Kaynak hesapta oluşan işlem kaydı.
        /// </summary>
        public virtual Transaction? FromTransaction { get; set; }
        /// <summary>
        /// Hedef hesapta oluşan işlem kaydı.
        /// </summary>
        public virtual Transaction? ToTransaction { get; set; }
    }
}