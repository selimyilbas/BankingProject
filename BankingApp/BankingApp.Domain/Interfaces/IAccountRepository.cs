using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Domain.Entities;

namespace BankingApp.Domain.Interfaces
{
    /// <summary>
    /// Hesaplara özel veri erişim işlemleri.
    /// </summary>
    public interface IAccountRepository : IGenericRepository<Account>
    {
        /// <summary>
        /// Hesap numarasına göre hesap getirir.
        /// </summary>
        Task<Account?> GetByAccountNumberAsync(string accountNumber);
        /// <summary>
        /// Müşteriye ait hesapları döner.
        /// </summary>
        Task<IEnumerable<Account>> GetAccountsByCustomerIdAsync(int customerId);
        /// <summary>
        /// Hesabı ilişkili işlemleriyle birlikte getirir.
        /// </summary>
        Task<Account?> GetAccountWithTransactionsAsync(int accountId);
        /// <summary>
        /// Para birimine göre benzersiz hesap numarası üretir.
        /// </summary>
        Task<string> GenerateAccountNumberAsync(string currency);
        /// <summary>
        /// Hesap bakiyesini günceller.
        /// </summary>
        Task<bool> UpdateBalanceAsync(int accountId, decimal newBalance);
    }
}
