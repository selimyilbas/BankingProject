using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Domain.Entities;

namespace BankingApp.Domain.Interfaces
{
    public interface ITransferRepository : IGenericRepository<Transfer>
    {
        Task<Transfer?> GetTransferWithDetailsAsync(int transferId);
        Task<IEnumerable<Transfer>> GetTransfersByAccountIdAsync(int accountId);
        Task<string> GenerateTransferCodeAsync();
    }
}
