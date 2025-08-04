namespace BankingApp.Application.DTOs.Account
{
    public class AccountBalanceDto
    {
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
}
