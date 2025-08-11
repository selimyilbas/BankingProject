using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Domain.Entities;

namespace BankingApp.Domain.Interfaces
{
    /// <summary>
    /// Transferlere özel veri erişim işlemleri.
    /// </summary>
    public interface ITransferRepository : IGenericRepository<Transfer>
    {
        /// <summary>
        /// Transferi ilişkili detayları ile getirir.
        /// </summary>
        Task<Transfer?> GetTransferWithDetailsAsync(int transferId);
        /// <summary>
        /// Hesaba ait transferleri döner.
        /// </summary>
        Task<IEnumerable<Transfer>> GetTransfersByAccountIdAsync(int accountId);
        Task<IEnumerable<Transfer>> GetByAccountAsync(int accountId);
        Task<IEnumerable<Transfer>> GetByCustomerAsync(int customerId);
        /// <summary>
        /// Yeni ve benzersiz transfer kodu üretir.
        /// </summary>
        Task<string> GenerateTransferCodeAsync();
    }
}
