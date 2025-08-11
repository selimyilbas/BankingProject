using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Account;
using BankingApp.Application.DTOs.Common;

namespace BankingApp.Application.Services.Interfaces
{
    /// <summary>
    /// Hesap işlemleri için uygulama katmanı sözleşmesi.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Yeni hesap oluşturur.
        /// </summary>
        /// <param name="dto">Oluşturma isteği.</param>
        /// <returns>Oluşturulan hesabı içeren yanıt.</returns>
        Task<ApiResponse<AccountDto>> CreateAccountAsync(CreateAccountDto dto);
        /// <summary>
        /// Hesap kimliğine göre hesabı getirir.
        /// </summary>
        Task<ApiResponse<AccountDto>> GetAccountByIdAsync(int accountId);
        /// <summary>
        /// Hesap numarasına göre hesabı getirir.
        /// </summary>
        Task<ApiResponse<AccountDto>> GetAccountByNumberAsync(string accountNumber);
        /// <summary>
        /// Müşterinin hesaplarını listeler.
        /// </summary>
        Task<ApiResponse<List<AccountDto>>> GetAccountsByCustomerIdAsync(int customerId);
        /// <summary>
        /// Hesap bakiyesini getirir.
        /// </summary>
        Task<ApiResponse<AccountBalanceDto>> GetAccountBalanceAsync(string accountNumber);
        /// <summary>
        /// Hesap aktiflik durumunu günceller.
        /// </summary>
        Task<ApiResponse<bool>> UpdateAccountStatusAsync(int accountId, bool isActive);
    }
}
