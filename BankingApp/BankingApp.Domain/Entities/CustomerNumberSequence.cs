// BankingApp.Domain/Entities/CustomerNumberSequence.cs
namespace BankingApp.Domain.Entities
{
    /// <summary>
    /// Müşteri numarası üreteci için sayaç.
    /// </summary>
    public class CustomerNumberSequence
    {
        public int Id { get; set; }
        public long LastNumber { get; set; }
    }
}