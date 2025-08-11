namespace BankingApp.Application.DTOs.Transfer
{
    /// <summary>
    /// Hesap kimlikleri ile transfer oluşturma isteği modeli.
    /// </summary>
    public class CreateTransferDto
    {
        public int FromAccountId { get; set; }
        public int ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}
