// BankingApp.Domain/Entities/Account.cs
using System;
using System.Collections.Generic;

namespace BankingApp.Domain.Entities
{
    /// <summary>
    /// Banka hesabı varlığı: bakiye, para birimi ve ilişkiler.
    /// </summary>
    public class Account
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Hesabın sahibi olan müşteri.
        /// </summary>
        public virtual Customer Customer { get; set; } = null!;
        /// <summary>
        /// Hesaba ait işlemler.
        /// </summary>
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        /// <summary>
        /// Bu hesaptan yapılan transferler.
        /// </summary>
        public virtual ICollection<Transfer> TransfersFrom { get; set; } = new List<Transfer>();
        /// <summary>
        /// Bu hesaba yapılan transferler.
        /// </summary>
        public virtual ICollection<Transfer> TransfersTo { get; set; } = new List<Transfer>();
    }
}