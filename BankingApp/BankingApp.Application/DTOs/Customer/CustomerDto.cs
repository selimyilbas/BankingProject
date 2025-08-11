using System;
using System.Collections.Generic;
using BankingApp.Application.DTOs.Account;

namespace BankingApp.Application.DTOs.Customer
{
    /// <summary>
    /// Müşteri detaylarını temsil eden DTO.
    /// </summary>
    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string CustomerNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string TCKN { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<AccountDto> Accounts { get; set; } = new();
    }
}
