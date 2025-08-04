using System.Threading.Tasks;
using BankingApp.Application.DTOs.Common;
using BankingApp.Application.DTOs.Customer;

namespace BankingApp.Application.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CreateCustomerDto dto);
        Task<ApiResponse<CustomerDto>> GetCustomerByIdAsync(int customerId);
        Task<ApiResponse<CustomerDto>> GetCustomerByNumberAsync(string customerNumber);
        Task<ApiResponse<CustomerDto>> GetCustomerByTCKNAsync(string tckn);
        Task<ApiResponse<CustomerWithAccountsDto>> GetCustomerWithAccountsAsync(int customerId);
        Task<ApiResponse<PagedResult<CustomerSummaryDto>>> GetAllCustomersAsync(int pageNumber, int pageSize);
        Task<ApiResponse<bool>> ValidateTCKNAsync(string tckn, string firstName, string lastName, int birthYear);
        Task<CustomerDto?> AuthenticateCustomer(string tckn, string password);
    }
}
