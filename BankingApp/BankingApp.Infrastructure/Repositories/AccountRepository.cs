using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Interfaces;
using BankingApp.Infrastructure.Data;
using System.Data;
using System;

namespace BankingApp.Infrastructure.Repositories
{
    public class AccountRepository : GenericRepository<Account>, IAccountRepository
    {
        public AccountRepository(BankingDbContext context) : base(context)
        {
        }

        public async Task<Account?> GetByAccountNumberAsync(string accountNumber)
        {
            return await _dbSet
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
        }

        public async Task<IEnumerable<Account>> GetAccountsByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Where(a => a.CustomerId == customerId)
                .Include(a => a.Customer)
                .ToListAsync();
        }

        public async Task<Account?> GetAccountWithTransactionsAsync(int accountId)
        {
            return await _dbSet
                .Include(a => a.Transactions)
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AccountId == accountId);
        }

        public async Task<string> GenerateAccountNumberAsync(string currency)
        {
            var currencyParam = new SqlParameter("@currencyType", currency);
            var accountNumberParam = new SqlParameter("@newNumber", SqlDbType.VarChar, 12)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC GenerateAccountNumber @currencyType, @newNumber OUTPUT",
                currencyParam,
                accountNumberParam);

            return accountNumberParam.Value?.ToString() ?? throw new InvalidOperationException("Failed to generate account number");
        }

        public async Task<bool> UpdateBalanceAsync(int accountId, decimal newBalance)
        {
            var account = await GetByIdAsync(accountId);
            if (account == null)
                return false;

            account.Balance = newBalance;
            account.UpdatedDate = DateTime.UtcNow;
            return true;
        }
    }
}
