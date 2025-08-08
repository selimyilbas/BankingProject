using System;

namespace BankingApp.Application.DTOs.Customer
{
    public class UpdateCustomerDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool? IsActive { get; set; }
    }
}


