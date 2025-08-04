using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Account;
using BankingApp.Application.DTOs.Common;

namespace BankingApp.Application.Services.Interfaces
{
    public interface IAccountService
    {
        Task<ApiResponse<AccountDto>> CreateAccountAsync(CreateAccountDto dto);
        Task<ApiResponse<AccountDto>> GetAccountByIdAsync(int accountId);
        Task<ApiResponse<AccountDto>> GetAccountByNumberAsync(string accountNumber);
        Task<ApiResponse<List<AccountDto>>> GetAccountsByCustomerIdAsync(int customerId);
        Task<ApiResponse<AccountBalanceDto>> GetAccountBalanceAsync(string accountNumber);
        Task<ApiResponse<bool>> UpdateAccountStatusAsync(int accountId, bool isActive);
    }
}
