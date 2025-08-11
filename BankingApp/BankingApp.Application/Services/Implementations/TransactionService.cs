using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BankingApp.Application.DTOs.Common;
using BankingApp.Application.DTOs.Transaction;
using BankingApp.Application.Services.Interfaces;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace BankingApp.Application.Services.Implementations
{
    /// <summary>
    /// İşlem (transaction) yönetimi iş kurallarını uygular.
    /// </summary>
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TransactionService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Para yatırma işlemini yürütür.
        /// </summary>
        public async Task<ApiResponse<TransactionDto>> DepositAsync(DepositDto dto)
        {
            try
            {
                // Validate amount
                if (dto.Amount <= 0)
                {
                    return ApiResponse<TransactionDto>.ErrorResponse("Amount must be greater than zero");
                }

                // Get account
                var account = await _unitOfWork.Accounts.GetByAccountNumberAsync(dto.AccountNumber);
                if (account == null)
                {
                    return ApiResponse<TransactionDto>.ErrorResponse("Account not found");
                }

                if (!account.IsActive)
                {
                    return ApiResponse<TransactionDto>.ErrorResponse("Account is not active");
                }

                // Begin transaction
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Generate transaction code
                    var transactionCode = await _unitOfWork.Transactions.GenerateTransactionCodeAsync();

                    // Create transaction record
                    var transaction = new Transaction
                    {
                        TransactionCode = transactionCode,
                        AccountId = account.AccountId,
                        TransactionType = "DEPOSIT",
                        Amount = dto.Amount,
                        Currency = account.Currency,
                        ExchangeRate = 1.0m,
                        Description = dto.Description ?? "Cash deposit",
                        TransactionDate = DateTime.UtcNow,
                        CreatedDate = DateTime.UtcNow
                    };

                    await _unitOfWork.Transactions.AddAsync(transaction);

                    // Update account balance
                    account.Balance += dto.Amount;
                    await _unitOfWork.Accounts.UpdateBalanceAsync(account.AccountId, account.Balance);

                    // Save changes
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    // Prepare response
                    transaction.Account = account;
                    var result = _mapper.Map<TransactionDto>(transaction);
                    return ApiResponse<TransactionDto>.SuccessResponse(result, "Deposit completed successfully");
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing deposit");
                return ApiResponse<TransactionDto>.ErrorResponse($"Error processing deposit: {ex.Message}");
            }
        }

        /// <summary>
        /// Hesaba ait tüm işlemleri döner.
        /// </summary>
        public async Task<ApiResponse<List<TransactionDto>>> GetTransactionsByAccountIdAsync(int accountId)
        {
            try
            {
                var transactions = await _unitOfWork.Transactions.GetTransactionsByAccountIdAsync(accountId);
                var result = _mapper.Map<List<TransactionDto>>(transactions);
                return ApiResponse<List<TransactionDto>>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions");
                return ApiResponse<List<TransactionDto>>.ErrorResponse("An error occurred while retrieving transactions");
            }
        }

        /// <summary>
        /// Tarih aralığına göre işlemleri döner.
        /// </summary>
        public async Task<ApiResponse<List<TransactionDto>>> GetTransactionsByDateRangeAsync(int accountId, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return ApiResponse<List<TransactionDto>>.ErrorResponse("Start date must be before end date");
                }

                var transactions = await _unitOfWork.Transactions.GetTransactionsByDateRangeAsync(accountId, startDate, endDate);
                var result = _mapper.Map<List<TransactionDto>>(transactions);
                return ApiResponse<List<TransactionDto>>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions by date range");
                return ApiResponse<List<TransactionDto>>.ErrorResponse("An error occurred while retrieving transactions");
            }
        }

        /// <summary>
        /// İşlemleri sayfalı olarak döner.
        /// </summary>
        public async Task<ApiResponse<PagedResult<TransactionDto>>> GetTransactionsPagedAsync(int accountId, int pageNumber, int pageSize)
        {
            try
            {
                var query = _unitOfWork.Transactions.Query()
                    .Where(t => t.AccountId == accountId)
                    .OrderByDescending(t => t.TransactionDate);

                var totalCount = query.Count();
                var transactions = query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var result = new PagedResult<TransactionDto>
                {
                    Items = _mapper.Map<List<TransactionDto>>(transactions),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return ApiResponse<PagedResult<TransactionDto>>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged transactions");
                return ApiResponse<PagedResult<TransactionDto>>.ErrorResponse("An error occurred while retrieving transactions");
            }
        }
    }
}
