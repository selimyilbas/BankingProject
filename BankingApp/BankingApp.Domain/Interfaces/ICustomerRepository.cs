using System.Threading.Tasks;
using BankingApp.Domain.Entities;

namespace BankingApp.Domain.Interfaces
{
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        Task<Customer?> GetByCustomerNumberAsync(string customerNumber);
        Task<Customer?> GetByTCKNAsync(string tckn);
        Task<Customer?> GetCustomerWithAccountsAsync(int customerId);
        Task<string> GenerateCustomerNumberAsync();
    }
}
