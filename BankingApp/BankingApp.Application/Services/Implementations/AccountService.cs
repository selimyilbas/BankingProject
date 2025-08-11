using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BankingApp.Application.DTOs.Account;
using BankingApp.Application.DTOs.Common;
using BankingApp.Application.Services.Interfaces;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace BankingApp.Application.Services.Implementations
{
    /// <summary>
    /// Hesap yönetimi iş kuralları ve veri erişim işlemlerini yürütür.
    /// </summary>
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountService> _logger;

        public AccountService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AccountService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Yeni bir hesap oluşturur ve numara atamasını gerçekleştirir.
        /// </summary>
        public async Task<ApiResponse<AccountDto>> CreateAccountAsync(CreateAccountDto dto)
        {
            try
            {
                var allowedCurrencies = new[] { "TL", "EUR", "USD" };
                if (!allowedCurrencies.Contains(dto.Currency))
                {
                    return ApiResponse<AccountDto>.ErrorResponse("Invalid currency. Allowed values: TL, EUR, USD");
                }

                var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId);
                if (customer == null || !customer.IsActive)
                {
                    return ApiResponse<AccountDto>.ErrorResponse("Customer not found or not active");
                }

                var existingAccounts = await _unitOfWork.Accounts.GetAccountsByCustomerIdAsync(dto.CustomerId);
                if (existingAccounts.Any(a => a.Currency == dto.Currency && a.IsActive))
                {
                    return ApiResponse<AccountDto>.ErrorResponse($"Customer already has an active {dto.Currency} account");
                }

                var accountNumber = await _unitOfWork.Accounts.GenerateAccountNumberAsync(dto.Currency);

                var account = new Account
                {
                    AccountNumber = accountNumber,
                    CustomerId = dto.CustomerId,
                    Currency = dto.Currency,
                    Balance = 0,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.Accounts.AddAsync(account);
                await _unitOfWork.SaveChangesAsync();

                account.Customer = customer;
                var result = _mapper.Map<AccountDto>(account);
                return ApiResponse<AccountDto>.SuccessResponse(result, "Account created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account");
                return ApiResponse<AccountDto>.ErrorResponse("An error occurred while creating account");
            }
        }

        /// <summary>
        /// Hesabı kimliğine göre döner.
        /// </summary>
        public async Task<ApiResponse<AccountDto>> GetAccountByIdAsync(int accountId)
        {
            try
            {
                var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);
                if (account == null)
                    return ApiResponse<AccountDto>.ErrorResponse("Account not found");

                var result = _mapper.Map<AccountDto>(account);
                return ApiResponse<AccountDto>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account by ID");
                return ApiResponse<AccountDto>.ErrorResponse("An error occurred while retrieving account");
            }
        }

        /// <summary>
        /// Hesabı numarasına göre döner.
        /// </summary>
        public async Task<ApiResponse<AccountDto>> GetAccountByNumberAsync(string accountNumber)
        {
            try
            {
                var account = await _unitOfWork.Accounts.GetByAccountNumberAsync(accountNumber);
                if (account == null)
                    return ApiResponse<AccountDto>.ErrorResponse("Account not found");

                var result = _mapper.Map<AccountDto>(account);
                return ApiResponse<AccountDto>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account by number");
                return ApiResponse<AccountDto>.ErrorResponse("An error occurred while retrieving account");
            }
        }

        /// <summary>
        /// Müşteriye ait hesapları listeler.
        /// </summary>
        public async Task<ApiResponse<List<AccountDto>>> GetAccountsByCustomerIdAsync(int customerId)
        {
            try
            {
                var accounts = await _unitOfWork.Accounts.GetAccountsByCustomerIdAsync(customerId);
                var result = _mapper.Map<List<AccountDto>>(accounts);
                return ApiResponse<List<AccountDto>>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accounts by customer ID");
                return ApiResponse<List<AccountDto>>.ErrorResponse("An error occurred while retrieving accounts");
            }
        }

        /// <summary>
        /// Hesap bakiyesini döner.
        /// </summary>
        public async Task<ApiResponse<AccountBalanceDto>> GetAccountBalanceAsync(string accountNumber)
        {
            try
            {
                var account = await _unitOfWork.Accounts.GetByAccountNumberAsync(accountNumber);
                if (account == null)
                    return ApiResponse<AccountBalanceDto>.ErrorResponse("Account not found");

                var result = _mapper.Map<AccountBalanceDto>(account);
                return ApiResponse<AccountBalanceDto>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account balance");
                return ApiResponse<AccountBalanceDto>.ErrorResponse("An error occurred while retrieving balance");
            }
        }

        /// <summary>
        /// Hesabın aktiflik durumunu günceller.
        /// </summary>
        public async Task<ApiResponse<bool>> UpdateAccountStatusAsync(int accountId, bool isActive)
        {
            try
            {
                var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);
                if (account == null)
                    return ApiResponse<bool>.ErrorResponse("Account not found");

                account.IsActive = isActive;
                account.UpdatedDate = DateTime.UtcNow;

                _unitOfWork.Accounts.Update(account);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, $"Account {(isActive ? "activated" : "deactivated")} successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating account status");
                return ApiResponse<bool>.ErrorResponse("An error occurred while updating account status");
            }
        }
    }
}
