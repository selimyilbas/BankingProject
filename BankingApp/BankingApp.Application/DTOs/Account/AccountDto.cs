using System;

namespace BankingApp.Application.DTOs.Account
{
    /// <summary>
    /// Hesap detaylarını temsil eden DTO.
    /// </summary>
    public class AccountDto
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
