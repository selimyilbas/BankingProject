using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using BankingApp.Domain.Interfaces;
using BankingApp.Infrastructure.Data;

namespace BankingApp.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BankingDbContext _context;
        private IDbContextTransaction? _transaction;
        
        private ICustomerRepository? _customerRepository;
        private IAccountRepository? _accountRepository;
        private ITransactionRepository? _transactionRepository;
        private ITransferRepository? _transferRepository;
        private IExchangeRateRepository? _exchangeRateRepository;

        public UnitOfWork(BankingDbContext context)
        {
            _context = context;
        }

        public ICustomerRepository Customers => 
            _customerRepository ??= new CustomerRepository(_context);

        public IAccountRepository Accounts => 
            _accountRepository ??= new AccountRepository(_context);

        public ITransactionRepository Transactions => 
            _transactionRepository ??= new TransactionRepository(_context);

        public ITransferRepository Transfers => 
            _transferRepository ??= new TransferRepository(_context);

        public IExchangeRateRepository ExchangeRates => 
            _exchangeRateRepository ??= new ExchangeRateRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
