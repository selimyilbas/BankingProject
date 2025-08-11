namespace BankingApp.Application.DTOs.Transfer
{
    /// <summary>
    /// Hesap numaraları ile transfer oluşturma isteği modeli.
    /// </summary>
    public class CreateTransferByAccountNumberDto
    {
        public string FromAccountNumber { get; set; } = string.Empty;
        public string ToAccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}


