// BankingApp.Domain/Entities/AccountNumberSequence.cs
namespace BankingApp.Domain.Entities
{
    /// <summary>
    /// Para birimine göre hesap numarası üreteci için sayaç.
    /// </summary>
    public class AccountNumberSequence
    {
        public int Id { get; set; }
        public string Currency { get; set; } = string.Empty;
        public long LastNumber { get; set; }
    }
}