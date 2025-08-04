// BankingApp.Domain/Entities/Account.cs
using System;
using System.Collections.Generic;

namespace BankingApp.Domain.Entities
{
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

        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public virtual ICollection<Transfer> TransfersFrom { get; set; } = new List<Transfer>();
        public virtual ICollection<Transfer> TransfersTo { get; set; } = new List<Transfer>();
    }
}