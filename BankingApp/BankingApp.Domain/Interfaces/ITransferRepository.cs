using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Domain.Entities;

namespace BankingApp.Domain.Interfaces
{
    public interface ITransferRepository : IGenericRepository<Transfer>
    {
        Task<Transfer?> GetTransferWithDetailsAsync(int transferId);
        Task<IEnumerable<Transfer>> GetTransfersByAccountIdAsync(int accountId);
        Task<IEnumerable<Transfer>> GetByAccountAsync(int accountId);
        Task<IEnumerable<Transfer>> GetByCustomerAsync(int customerId);
        Task<string> GenerateTransferCodeAsync();
    }
}
