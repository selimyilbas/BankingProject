using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BankingApp.Application.DTOs.Common;
using BankingApp.Application.DTOs.Customer;
using BankingApp.Application.Services.Interfaces;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace BankingApp.Application.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerService> _logger;
        private readonly IEncryptionService? _encryptionService;

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CustomerService> logger, IEncryptionService? encryptionService = null)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _encryptionService = encryptionService;
        }

        public async Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CreateCustomerDto dto)
        {
            try
            {
                // Check if TCKN already exists
                var existingCustomer = await _unitOfWork.Customers.GetByTCKNAsync(dto.TCKN);
                if (existingCustomer != null)
                {
                    return ApiResponse<CustomerDto>.ErrorResponse("Customer with this TCKN already exists");
                }

                // TODO: Validate TCKN with MERNIS service
                // For now, we'll skip this validation

                // Generate customer number
                var customerNumber = await _unitOfWork.Customers.GenerateCustomerNumberAsync();

                // Create new customer
                var customer = _mapper.Map<Customer>(dto);
                customer.CustomerNumber = customerNumber;
                // Encrypt password if service available (backward compatible if not configured)
                customer.Password = _encryptionService != null ? _encryptionService.Encrypt(dto.Password) : dto.Password;
                customer.IsActive = true;
                customer.CreatedDate = DateTime.UtcNow;

                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<CustomerDto>(customer);
                return ApiResponse<CustomerDto>.SuccessResponse(result, "Customer created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                return ApiResponse<CustomerDto>.ErrorResponse("An error occurred while creating customer");
            }
        }

        public async Task<ApiResponse<CustomerDto>> GetCustomerByIdAsync(int customerId)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetCustomerWithAccountsAsync(customerId);
                if (customer == null)
                {
                    return ApiResponse<CustomerDto>.ErrorResponse("Customer not found");
                }

                var result = _mapper.Map<CustomerDto>(customer);
                return ApiResponse<CustomerDto>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by ID");
                return ApiResponse<CustomerDto>.ErrorResponse("An error occurred while retrieving customer");
            }
        }

        public async Task<ApiResponse<CustomerDto>> GetCustomerByNumberAsync(string customerNumber)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByCustomerNumberAsync(customerNumber);
                if (customer == null)
                {
                    return ApiResponse<CustomerDto>.ErrorResponse("Customer not found");
                }

                var result = _mapper.Map<CustomerDto>(customer);
                return ApiResponse<CustomerDto>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by number");
                return ApiResponse<CustomerDto>.ErrorResponse("An error occurred while retrieving customer");
            }
        }

        public async Task<ApiResponse<CustomerDto>> GetCustomerByTCKNAsync(string tckn)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByTCKNAsync(tckn);
                if (customer == null)
                {
                    return ApiResponse<CustomerDto>.ErrorResponse("Customer not found");
                }

                var result = _mapper.Map<CustomerDto>(customer);
                return ApiResponse<CustomerDto>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by TCKN");
                return ApiResponse<CustomerDto>.ErrorResponse("An error occurred while retrieving customer");
            }
        }

        public async Task<ApiResponse<CustomerWithAccountsDto>> GetCustomerWithAccountsAsync(int customerId)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetCustomerWithAccountsAsync(customerId);
                if (customer == null)
                {
                    return ApiResponse<CustomerWithAccountsDto>.ErrorResponse("Customer not found");
                }

                var result = _mapper.Map<CustomerWithAccountsDto>(customer);
                return ApiResponse<CustomerWithAccountsDto>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer with accounts");
                return ApiResponse<CustomerWithAccountsDto>.ErrorResponse("An error occurred while retrieving customer");
            }
        }

        public async Task<ApiResponse<PagedResult<CustomerSummaryDto>>> GetAllCustomersAsync(int pageNumber, int pageSize)
        {
            try
            {
                var query = _unitOfWork.Customers.Query()
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.CustomerId);

                var totalCount = query.Count();
                var customers = query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var result = new PagedResult<CustomerSummaryDto>
                {
                    Items = _mapper.Map<List<CustomerSummaryDto>>(customers),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return ApiResponse<PagedResult<CustomerSummaryDto>>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all customers");
                return ApiResponse<PagedResult<CustomerSummaryDto>>.ErrorResponse("An error occurred while retrieving customers");
            }
        }

        public async Task<ApiResponse<bool>> ValidateTCKNAsync(string tckn, string firstName, string lastName, int birthYear)
        {
            try
            {
                // TODO: Implement MERNIS validation
                // For now, return true
                await Task.Delay(100); // Simulate API call
                return ApiResponse<bool>.SuccessResponse(true, "TCKN validation successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating TCKN");
                return ApiResponse<bool>.ErrorResponse("An error occurred during TCKN validation");
            }
        }

        public async Task<CustomerDto?> AuthenticateCustomer(string tckn, string password)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByTCKNAsync(tckn);
                
                if (customer == null)
                {
                    return null;
                }

                string stored = customer.Password ?? string.Empty;
                bool isValid;
                if (_encryptionService != null && _encryptionService.IsEncrypted(stored))
                {
                    isValid = _encryptionService.Decrypt(stored) == password;
                }
                else
                {
                    isValid = stored == password;
                }

                if (!isValid)
                {
                    return null;
                }

                // Lazy migrate if plain
                if (_encryptionService != null && !_encryptionService.IsEncrypted(stored))
                {
                    try
                    {
                        customer.Password = _encryptionService.Encrypt(password);
                        _unitOfWork.Customers.Update(customer);
                        await _unitOfWork.SaveChangesAsync();
                    }
                    catch { }
                }

                return _mapper.Map<CustomerDto>(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating customer");
                return null;
            }
        }

        public async Task<ApiResponse<CustomerDto>> UpdateCustomerAsync(int customerId, UpdateCustomerDto dto)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
                if (customer == null)
                {
                    return ApiResponse<CustomerDto>.ErrorResponse("Customer not found");
                }

                customer.FirstName = dto.FirstName;
                customer.LastName = dto.LastName;
                customer.Email = dto.Email;
                customer.PhoneNumber = dto.PhoneNumber;
                customer.DateOfBirth = dto.DateOfBirth;
                if (dto.IsActive.HasValue)
                {
                    customer.IsActive = dto.IsActive.Value;
                }

                _unitOfWork.Customers.Update(customer);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<CustomerDto>(customer);
                return ApiResponse<CustomerDto>.SuccessResponse(result, "Customer updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer");
                return ApiResponse<CustomerDto>.ErrorResponse("An error occurred while updating customer");
            }
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(int customerId, string currentPassword, string newPassword)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
                if (customer == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Customer not found");
                }

                string storedPassword = customer.Password ?? string.Empty;
                string currentPlain = storedPassword;
                if (_encryptionService != null && _encryptionService.IsEncrypted(storedPassword))
                {
                    currentPlain = _encryptionService.Decrypt(storedPassword);
                }

                if (currentPlain != currentPassword)
                {
                    return ApiResponse<bool>.ErrorResponse("Current password is incorrect");
                }

                customer.Password = _encryptionService != null ? _encryptionService.Encrypt(newPassword) : newPassword;
                _unitOfWork.Customers.Update(customer);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Password changed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return ApiResponse<bool>.ErrorResponse("An error occurred while changing password");
            }
        }
    }
}
