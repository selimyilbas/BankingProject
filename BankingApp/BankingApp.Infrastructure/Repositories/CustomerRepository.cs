using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Interfaces;
using BankingApp.Infrastructure.Data;
using System.Data;

namespace BankingApp.Infrastructure.Repositories
{
    /// <summary>
    /// Müşterilere yönelik veri erişim işlemleri.
    /// </summary>
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        /// <summary>
        /// Repository örneğini başlatır.
        /// </summary>
        public CustomerRepository(BankingDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Müşteri numarasına göre müşteri getirir.
        /// </summary>
        public async Task<Customer?> GetByCustomerNumberAsync(string customerNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.CustomerNumber == customerNumber);
        }

        /// <summary>
        /// TCKN'ye göre müşteri getirir.
        /// </summary>
        public async Task<Customer?> GetByTCKNAsync(string tckn)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.TCKN == tckn);
        }

        /// <summary>
        /// Müşteriyi hesaplarıyla birlikte getirir.
        /// </summary>
        public async Task<Customer?> GetCustomerWithAccountsAsync(int customerId)
        {
            return await _dbSet
                .Include(c => c.Accounts)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }
        /// <summary>
        /// Sıralı yeni müşteri numarası üretir (SP çağrısı).
        /// </summary>
        public async Task<string> GenerateCustomerNumberAsync()
        {
            var customerNumberParam = new SqlParameter("@newNumber", SqlDbType.VarChar, 12)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC GenerateCustomerNumber @newNumber OUTPUT",
                customerNumberParam);

            return customerNumberParam.Value?.ToString() ?? throw new InvalidOperationException("Failed to generate customer number");
        }
    }
}
