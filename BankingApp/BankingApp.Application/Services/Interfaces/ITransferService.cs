using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Common;
using BankingApp.Application.DTOs.Transfer;

namespace BankingApp.Application.Services.Interfaces
{
    /// <summary>
    /// Para transferi yönetimi için servis sözleşmesi.
    /// </summary>
    public interface ITransferService
    {
        /// <summary>
        /// Hesap kimlikleri ile transfer oluşturur.
        /// </summary>
        Task<ApiResponse<TransferDto>> CreateTransferAsync(CreateTransferDto dto);
        /// <summary>
        /// Hesap numaraları ile transfer oluşturur.
        /// </summary>
        Task<ApiResponse<TransferDto>> CreateTransferByAccountNumberAsync(string fromAccountNumber, string toAccountNumber, decimal amount, string? description);
        /// <summary>
        /// Transfer kimliğine göre transfer getirir.
        /// </summary>
        Task<ApiResponse<TransferDto>> GetTransferByIdAsync(int transferId);
        /// <summary>
        /// Hesaba ait transferleri listeler.
        /// </summary>
        Task<ApiResponse<List<TransferDto>>> GetTransfersByAccountAsync(int accountId);
        /// <summary>
        /// Müşteriye ait transferleri listeler.
        /// </summary>
        Task<ApiResponse<List<TransferDto>>> GetTransfersByCustomerAsync(int customerId);
        /// <summary>
        /// Transfer kurallarını doğrular.
        /// </summary>
        Task<ApiResponse<object>> ValidateTransferAsync(CreateTransferDto dto);
        /// <summary>
        /// Hesap numaraları ile transfer kurallarını doğrular.
        /// </summary>
        Task<ApiResponse<object>> ValidateTransferByAccountNumberAsync(string fromAccountNumber, string toAccountNumber, decimal amount);
    }
}
