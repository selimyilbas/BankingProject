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
    /// Transferlere yönelik veri erişim işlemleri.
    /// </summary>
    public class TransferRepository : GenericRepository<Transfer>, ITransferRepository
    {
        /// <summary>
        /// Repository örneğini başlatır.
        /// </summary>
        public TransferRepository(BankingDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Transferi ilişkili hesap ve işlem detaylarıyla birlikte getirir.
        /// </summary>
        public async Task<Transfer?> GetTransferWithDetailsAsync(int transferId)
        {
            return await _dbSet
                .Include(t => t.FromAccount)
                    .ThenInclude(a => a.Customer)
                .Include(t => t.ToAccount)
                    .ThenInclude(a => a.Customer)
                .Include(t => t.FromTransaction)
                .Include(t => t.ToTransaction)
                .FirstOrDefaultAsync(t => t.TransferId == transferId);
        }

        /// <summary>
        /// Hesaba ait transferleri döner.
        /// </summary>
        public async Task<IEnumerable<Transfer>> GetTransfersByAccountIdAsync(int accountId)
        {
            return await _dbSet
                .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
                .Include(t => t.FromAccount)
                .Include(t => t.ToAccount)
                .OrderByDescending(t => t.TransferDate)
                .ToListAsync();
        }

        /// <summary>
        /// Hesabın taraf olduğu transferleri döner.
        /// </summary>
        public async Task<IEnumerable<Transfer>> GetByAccountAsync(int accountId)
        {
            return await _dbSet
                .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
                .Include(t => t.FromAccount)
                .Include(t => t.ToAccount)
                .OrderByDescending(t => t.TransferDate)
                .ToListAsync();
        }

        /// <summary>
        /// Müşteriye ait transferleri döner.
        /// </summary>
        public async Task<IEnumerable<Transfer>> GetByCustomerAsync(int customerId)
        {
            return await _dbSet
                .Include(t => t.FromAccount)
                .Include(t => t.ToAccount)
                .Where(t => t.FromAccount.CustomerId == customerId || t.ToAccount.CustomerId == customerId)
                .OrderByDescending(t => t.TransferDate)
                .ToListAsync();
        }

        /// <summary>
        /// Benzersiz transfer kodu üretir.
        /// </summary>
        public async Task<string> GenerateTransferCodeAsync()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(10, 99); // 2 haneli
            return await Task.FromResult($"TRF{timestamp}{random}");
        }
    }
}
