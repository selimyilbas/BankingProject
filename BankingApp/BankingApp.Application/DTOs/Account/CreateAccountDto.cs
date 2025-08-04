namespace BankingApp.Application.DTOs.Account
{
    public class CreateAccountDto
    {
        public int CustomerId { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
}
