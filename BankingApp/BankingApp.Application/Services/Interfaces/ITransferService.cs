using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Common;
using BankingApp.Application.DTOs.Transfer;

namespace BankingApp.Application.Services.Interfaces
{
    public interface ITransferService
    {
        Task<ApiResponse<TransferDto>> CreateTransferAsync(CreateTransferDto dto);
        Task<ApiResponse<TransferDto>> GetTransferByIdAsync(int transferId);
        Task<ApiResponse<List<TransferDto>>> GetTransfersByAccountIdAsync(int accountId);
        Task<ApiResponse<PagedResult<TransferDto>>> GetTransfersPagedAsync(int accountId, int pageNumber, int pageSize);
    }
}
