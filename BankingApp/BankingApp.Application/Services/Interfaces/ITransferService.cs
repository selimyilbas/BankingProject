using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Common;
using BankingApp.Application.DTOs.Transfer;

namespace BankingApp.Application.Services.Interfaces
{
    public interface ITransferService
    {
        Task<ApiResponse<TransferDto>> CreateTransferAsync(CreateTransferDto dto);
        Task<ApiResponse<TransferDto>> CreateTransferByAccountNumberAsync(string fromAccountNumber, string toAccountNumber, decimal amount, string? description);
        Task<ApiResponse<TransferDto>> GetTransferByIdAsync(int transferId);
        Task<ApiResponse<List<TransferDto>>> GetTransfersByAccountAsync(int accountId);
        Task<ApiResponse<List<TransferDto>>> GetTransfersByCustomerAsync(int customerId);
        Task<ApiResponse<object>> ValidateTransferAsync(CreateTransferDto dto);
        Task<ApiResponse<object>> ValidateTransferByAccountNumberAsync(string fromAccountNumber, string toAccountNumber, decimal amount);
    }
}
