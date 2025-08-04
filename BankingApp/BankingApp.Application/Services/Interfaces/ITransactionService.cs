using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Common;
using BankingApp.Application.DTOs.Transaction;

namespace BankingApp.Application.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<ApiResponse<TransactionDto>> DepositAsync(DepositDto dto);
        Task<ApiResponse<List<TransactionDto>>> GetTransactionsByAccountIdAsync(int accountId);
        Task<ApiResponse<List<TransactionDto>>> GetTransactionsByDateRangeAsync(int accountId, DateTime startDate, DateTime endDate);
        Task<ApiResponse<PagedResult<TransactionDto>>> GetTransactionsPagedAsync(int accountId, int pageNumber, int pageSize);
    }
}
