using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Common;
using BankingApp.Application.DTOs.Transaction;

namespace BankingApp.Application.Services.Interfaces
{
    /// <summary>
    /// İşlem yönetimi için servis sözleşmesi.
    /// </summary>
    public interface ITransactionService
    {
        /// <summary>
        /// Para yatırma işlemi yapar.
        /// </summary>
        Task<ApiResponse<TransactionDto>> DepositAsync(DepositDto dto);
        /// <summary>
        /// Hesaba ait işlemleri listeler.
        /// </summary>
        Task<ApiResponse<List<TransactionDto>>> GetTransactionsByAccountIdAsync(int accountId);
        /// <summary>
        /// Tarih aralığına göre işlemleri listeler.
        /// </summary>
        Task<ApiResponse<List<TransactionDto>>> GetTransactionsByDateRangeAsync(int accountId, DateTime startDate, DateTime endDate);
        /// <summary>
        /// İşlemleri sayfalı olarak listeler.
        /// </summary>
        Task<ApiResponse<PagedResult<TransactionDto>>> GetTransactionsPagedAsync(int accountId, int pageNumber, int pageSize);
    }
}
