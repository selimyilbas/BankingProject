using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Domain.Entities;

namespace BankingApp.Domain.Interfaces
{
    /// <summary>
    /// İşlemlere (transaction) özel veri erişim işlemleri.
    /// </summary>
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {
        /// <summary>
        /// Hesaba ait işlemleri döner.
        /// </summary>
        Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(int accountId);
        /// <summary>
        /// Tarih aralığına göre işlemleri döner.
        /// </summary>
        Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(int accountId, DateTime startDate, DateTime endDate);
        /// <summary>
        /// Yeni ve benzersiz işlem kodu üretir.
        /// </summary>
        Task<string> GenerateTransactionCodeAsync();
    }
}
