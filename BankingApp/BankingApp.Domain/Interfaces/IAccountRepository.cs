using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Domain.Entities;

namespace BankingApp.Domain.Interfaces
{
    public interface IAccountRepository : IGenericRepository<Account>
    {
        Task<Account?> GetByAccountNumberAsync(string accountNumber);
        Task<IEnumerable<Account>> GetAccountsByCustomerIdAsync(int customerId);
        Task<Account?> GetAccountWithTransactionsAsync(int accountId);
        Task<string> GenerateAccountNumberAsync(string currency);
        Task<bool> UpdateBalanceAsync(int accountId, decimal newBalance);
    }
}
