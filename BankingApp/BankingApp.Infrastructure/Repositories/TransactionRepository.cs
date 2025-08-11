using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Interfaces;
using BankingApp.Infrastructure.Data;

namespace BankingApp.Infrastructure.Repositories
{
    /// <summary>
    /// İşlemlere yönelik veri erişim işlemleri.
    /// </summary>
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        /// <summary>
        /// Repository örneğini başlatır.
        /// </summary>
        public TransactionRepository(BankingDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Hesaba ait işlemleri döner.
        /// </summary>
        public async Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(int accountId)
        {
            return await _dbSet
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        /// <summary>
        /// Tarih aralığına göre işlemleri döner.
        /// </summary>
        public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(int accountId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(t => t.AccountId == accountId && 
                           t.TransactionDate >= startDate && 
                           t.TransactionDate <= endDate)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        /// <summary>
        /// Benzersiz işlem kodu üretir.
        /// </summary>
        public async Task<string> GenerateTransactionCodeAsync()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(10, 99); // keep code length <= 20
            return await Task.FromResult($"TRX{timestamp}{random}");
        }
    }
}
