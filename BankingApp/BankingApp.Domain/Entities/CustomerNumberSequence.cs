// BankingApp.Domain/Entities/CustomerNumberSequence.cs
namespace BankingApp.Domain.Entities
{
    public class CustomerNumberSequence
    {
        public int Id { get; set; }
        public long LastNumber { get; set; }
    }
}