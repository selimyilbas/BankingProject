namespace BankingApp.Application.DTOs.Transfer
{
    public class CreateTransferDto
    {
        public int FromAccountId { get; set; }
        public int ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}
