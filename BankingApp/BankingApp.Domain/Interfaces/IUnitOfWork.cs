using System;
using System.Threading.Tasks;

namespace BankingApp.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICustomerRepository Customers { get; }
        IAccountRepository Accounts { get; }
        ITransactionRepository Transactions { get; }
        ITransferRepository Transfers { get; }
        IExchangeRateRepository ExchangeRates { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
