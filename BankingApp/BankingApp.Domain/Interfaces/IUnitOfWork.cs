using System;
using System.Threading.Tasks;

namespace BankingApp.Domain.Interfaces
{
    /// <summary>
    /// İşlem (transaction) sınırı ve repository erişimleri için birim.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Müşteri deposu.
        /// </summary>
        ICustomerRepository Customers { get; }
        /// <summary>
        /// Hesap deposu.
        /// </summary>
        IAccountRepository Accounts { get; }
        /// <summary>
        /// İşlem (transaction) deposu.
        /// </summary>
        ITransactionRepository Transactions { get; }
        /// <summary>
        /// Transfer deposu.
        /// </summary>
        ITransferRepository Transfers { get; }
        /// <summary>
        /// Döviz kuru deposu.
        /// </summary>
        IExchangeRateRepository ExchangeRates { get; }
        
        /// <summary>
        /// Değişiklikleri kalıcı hale getirir.
        /// </summary>
        Task<int> SaveChangesAsync();
        /// <summary>
        /// Yeni bir veritabanı işlemi başlatır.
        /// </summary>
        Task BeginTransactionAsync();
        /// <summary>
        /// Açık olan işlemi başarılı şekilde sonlandırır.
        /// </summary>
        Task CommitTransactionAsync();
        /// <summary>
        /// Açık olan işlemi geri alır.
        /// </summary>
        Task RollbackTransactionAsync();
    }
}
