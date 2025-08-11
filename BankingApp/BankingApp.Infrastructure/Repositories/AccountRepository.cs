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
    /// <summary>
    /// Hesaplara yönelik veri erişim işlemleri.
    /// </summary>
    public class AccountRepository : GenericRepository<Account>, IAccountRepository
    {
        /// <summary>
        /// Repository örneğini başlatır.
        /// </summary>
        public AccountRepository(BankingDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Hesap numarasına göre hesap getirir.
        /// </summary>
        public async Task<Account?> GetByAccountNumberAsync(string accountNumber)
        {
            return await _dbSet
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
        }

        /// <summary>
        /// Müşterinin tüm hesaplarını getirir.
        /// </summary>
        public async Task<IEnumerable<Account>> GetAccountsByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Where(a => a.CustomerId == customerId)
                .Include(a => a.Customer)
                .ToListAsync();
        }

        /// <summary>
        /// Hesabı işlemleriyle birlikte getirir.
        /// </summary>
        public async Task<Account?> GetAccountWithTransactionsAsync(int accountId)
        {
            return await _dbSet
                .Include(a => a.Transactions)
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AccountId == accountId);
        }

        /// <summary>
        /// Para birimine göre yeni hesap numarası üretir (SP çağrısı).
        /// </summary>
        public async Task<string> GenerateAccountNumberAsync(string currency)
        {
            // Ensure a sequence row exists for the requested currency to avoid NULL from the stored procedure
            var sequenceExists = await _context.AccountNumberSequences
                .AnyAsync(s => s.Currency == currency);
            if (!sequenceExists)
            {
                _context.AccountNumberSequences.Add(new AccountNumberSequence
                {
                    Currency = currency,
                    LastNumber = 0
                });
                await _context.SaveChangesAsync();
            }

            var currencyParam = new SqlParameter("@currencyType", currency);
            var accountNumberParam = new SqlParameter("@newNumber", SqlDbType.VarChar, 12)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC GenerateAccountNumber @currencyType, @newNumber OUTPUT",
                currencyParam,
                accountNumberParam);

            var accountNumber = accountNumberParam.Value?.ToString();
            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                throw new InvalidOperationException("Failed to generate account number");
            }
            return accountNumber;
        }

        /// <summary>
        /// Hesap bakiyesini günceller.
        /// </summary>
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
