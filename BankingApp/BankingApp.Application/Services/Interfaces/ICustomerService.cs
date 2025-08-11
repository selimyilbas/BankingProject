using System.Threading.Tasks;
using BankingApp.Application.DTOs.Common;
using BankingApp.Application.DTOs.Customer;

namespace BankingApp.Application.Services.Interfaces
{
    /// <summary>
    /// Müşteri yönetimi için uygulama katmanı sözleşmesi.
    /// </summary>
    public interface ICustomerService
    {
        /// <summary>
        /// Yeni müşteri oluşturur.
        /// </summary>
        Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CreateCustomerDto dto);
        /// <summary>
        /// Müşteri kimliğine göre müşteriyi getirir.
        /// </summary>
        Task<ApiResponse<CustomerDto>> GetCustomerByIdAsync(int customerId);
        /// <summary>
        /// Müşteri numarasına göre müşteriyi getirir.
        /// </summary>
        Task<ApiResponse<CustomerDto>> GetCustomerByNumberAsync(string customerNumber);
        /// <summary>
        /// TCKN'ye göre müşteriyi getirir.
        /// </summary>
        Task<ApiResponse<CustomerDto>> GetCustomerByTCKNAsync(string tckn);
        /// <summary>
        /// Müşteriyi hesaplarıyla birlikte getirir.
        /// </summary>
        Task<ApiResponse<CustomerWithAccountsDto>> GetCustomerWithAccountsAsync(int customerId);
        /// <summary>
        /// Müşteri listesini sayfalı olarak döner.
        /// </summary>
        Task<ApiResponse<PagedResult<CustomerSummaryDto>>> GetAllCustomersAsync(int pageNumber, int pageSize);
        /// <summary>
        /// TCKN doğrulaması yapar.
        /// </summary>
        Task<ApiResponse<bool>> ValidateTCKNAsync(string tckn, string firstName, string lastName, int birthYear);
        /// <summary>
        /// TCKN ve şifre ile müşteri kimlik doğrulaması yapar.
        /// </summary>
        Task<CustomerDto?> AuthenticateCustomer(string tckn, string password);
        /// <summary>
        /// Müşteri bilgilerini günceller.
        /// </summary>
        Task<ApiResponse<CustomerDto>> UpdateCustomerAsync(int customerId, UpdateCustomerDto dto);
        /// <summary>
        /// Müşteri şifresini değiştirir.
        /// </summary>
        Task<ApiResponse<bool>> ChangePasswordAsync(int customerId, string currentPassword, string newPassword);
    }
}
