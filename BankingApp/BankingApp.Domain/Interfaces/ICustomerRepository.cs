using System.Threading.Tasks;
using BankingApp.Domain.Entities;

namespace BankingApp.Domain.Interfaces
{
    /// <summary>
    /// Müşterilere özel veri erişim işlemleri.
    /// </summary>
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        /// <summary>
        /// Müşteri numarasına göre müşteri getirir.
        /// </summary>
        Task<Customer?> GetByCustomerNumberAsync(string customerNumber);
        /// <summary>
        /// TCKN'ye göre müşteri getirir.
        /// </summary>
        Task<Customer?> GetByTCKNAsync(string tckn);
        /// <summary>
        /// Müşteriyi hesaplarıyla birlikte getirir.
        /// </summary>
        Task<Customer?> GetCustomerWithAccountsAsync(int customerId);
        /// <summary>
        /// Yeni ve benzersiz müşteri numarası üretir.
        /// </summary>
        Task<string> GenerateCustomerNumberAsync();
    }
}
