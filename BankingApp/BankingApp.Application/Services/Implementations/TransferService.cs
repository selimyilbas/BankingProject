// BankingApp.Application/Services/Implementations/TransferService.cs
using System;
using System.Threading.Tasks;
using AutoMapper;
using BankingApp.Application.DTOs.Common;
using BankingApp.Application.DTOs.Transfer;
using BankingApp.Application.Services.Interfaces;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace BankingApp.Application.Services.Implementations
{
    public class TransferService : ITransferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TransferService> _logger;
        private readonly IExchangeRateService _exchangeRateService;

        public TransferService(
            IUnitOfWork unitOfWork, 
            IMapper mapper, 
            ILogger<TransferService> logger,
            IExchangeRateService exchangeRateService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _exchangeRateService = exchangeRateService;
        }

        public async Task<ApiResponse<TransferDto>> CreateTransferAsync(CreateTransferDto dto)
        {
            try
            {
                // Validate amount
                if (dto.Amount <= 0)
                {
                    return ApiResponse<TransferDto>.ErrorResponse("Amount must be greater than zero");
                }

                // Get accounts
                var fromAccount = await _unitOfWork.Accounts.GetByAccountNumberAsync(dto.FromAccountNumber);
                var toAccount = await _unitOfWork.Accounts.GetByAccountNumberAsync(dto.ToAccountNumber);

                if (fromAccount == null)
                {
                    return ApiResponse<TransferDto>.ErrorResponse("Source account not found");
                }

                if (toAccount == null)
                {
                    return ApiResponse<TransferDto>.ErrorResponse("Destination account not found");
                }

                if (!fromAccount.IsActive || !toAccount.IsActive)
                {
                    return ApiResponse<TransferDto>.ErrorResponse("One or both accounts are inactive");
                }

                // Check balance
                if (fromAccount.Balance < dto.Amount)
                {
                    return ApiResponse<TransferDto>.ErrorResponse("Insufficient balance");
                }

                // Begin transaction
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Generate transfer code
                    var transferCode = await _unitOfWork.Transfers.GenerateTransferCodeAsync();

                    // Calculate exchange rate if different currencies
                    decimal exchangeRate = 1.0m;
                    decimal convertedAmount = dto.Amount;

                    if (fromAccount.Currency != toAccount.Currency)
                    {
                        var rateResponse = await _exchangeRateService.GetExchangeRateAsync(
                            fromAccount.Currency, 
                            toAccount.Currency);
                        
                        if (!rateResponse.Success || rateResponse.Data == 0)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return ApiResponse<TransferDto>.ErrorResponse("Failed to get exchange rate");
                        }

                        exchangeRate = rateResponse.Data;
                        convertedAmount = dto.Amount * exchangeRate;
                    }

                    // Create transfer record
                    var transfer = new Transfer
                    {
                        TransferCode = transferCode,
                        FromAccountId = fromAccount.AccountId,
                        ToAccountId = toAccount.AccountId,
                        Amount = dto.Amount,
                        FromCurrency = fromAccount.Currency,
                        ToCurrency = toAccount.Currency,
                        ExchangeRate = exchangeRate,
                        ConvertedAmount = convertedAmount,
                        Status = "PENDING",
                        Description = dto.Description,
                        TransferDate = DateTime.UtcNow
                    };

                    await _unitOfWork.Transfers.AddAsync(transfer);

                    // Create debit transaction
                    var debitTransactionCode = await _unitOfWork.Transactions.GenerateTransactionCodeAsync();
                    var debitTransaction = new Transaction
                    {
                        TransactionCode = debitTransactionCode,
                        AccountId = fromAccount.AccountId,
                        TransactionType = "TRANSFER_OUT",
                        Amount = dto.Amount,
                        Currency = fromAccount.Currency,
                        ExchangeRate = 1.0m,
                        Description = $"Transfer to {toAccount.AccountNumber}: {dto.Description}",
                        TransactionDate = DateTime.UtcNow,
                        CreatedDate = DateTime.UtcNow
                    };

                    await _unitOfWork.Transactions.AddAsync(debitTransaction);

                    // Create credit transaction
                    var creditTransactionCode = await _unitOfWork.Transactions.GenerateTransactionCodeAsync();
                    var creditTransaction = new Transaction
                    {
                        TransactionCode = creditTransactionCode,
                        AccountId = toAccount.AccountId,
                        TransactionType = "TRANSFER_IN",
                        Amount = convertedAmount,
                        Currency = toAccount.Currency,
                        ExchangeRate = exchangeRate,
                        Description = $"Transfer from {fromAccount.AccountNumber}: {dto.Description}",
                        TransactionDate = DateTime.UtcNow,
                        CreatedDate = DateTime.UtcNow
                    };

                    await _unitOfWork.Transactions.AddAsync(creditTransaction);

                    // Update balances
                    fromAccount.Balance -= dto.Amount;
                    toAccount.Balance += convertedAmount;

                    await _unitOfWork.Accounts.UpdateBalanceAsync(fromAccount.AccountId, fromAccount.Balance);
                    await _unitOfWork.Accounts.UpdateBalanceAsync(toAccount.AccountId, toAccount.Balance);

                    // Update transfer status
                    transfer.Status = "COMPLETED";
                    transfer.CompletedDate = DateTime.UtcNow;

                    // Save changes
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    // Prepare response
                    transfer.FromAccount = fromAccount;
                    transfer.ToAccount = toAccount;
                    var result = _mapper.Map<TransferDto>(transfer);
                    return ApiResponse<TransferDto>.SuccessResponse(result, "Transfer completed successfully");
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing transfer");
                return ApiResponse<TransferDto>.ErrorResponse("An error occurred while processing transfer");
            }
        }

        public async Task<ApiResponse<TransferDto>> GetTransferByIdAsync(int transferId)
        {
            try
            {
                var transfer = await _unitOfWork.Transfers.GetTransferWithDetailsAsync(transferId);
                if (transfer == null)
                {
                    return ApiResponse<TransferDto>.ErrorResponse("Transfer not found");
                }

                var result = _mapper.Map<TransferDto>(transfer);
                return ApiResponse<TransferDto>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transfer");
                return ApiResponse<TransferDto>.ErrorResponse("An error occurred while retrieving transfer");
            }
        }

        public async Task<ApiResponse<List<TransferDto>>> GetTransfersByAccountIdAsync(int accountId)
        {
            try
            {
                var transfers = await _unitOfWork.Transfers.GetTransfersByAccountIdAsync(accountId);
                var result = _mapper.Map<List<TransferDto>>(transfers);
                return ApiResponse<List<TransferDto>>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transfers");
                return ApiResponse<List<TransferDto>>.ErrorResponse("An error occurred while retrieving transfers");
            }
        }

        public async Task<ApiResponse<PagedResult<TransferDto>>> GetTransfersPagedAsync(int accountId, int pageNumber, int pageSize)
        {
            try
            {
                var query = _unitOfWork.Transfers.Query()
                    .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
                    .OrderByDescending(t => t.TransferDate);

                var totalCount = query.Count();
                var transfers = query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var result = new PagedResult<TransferDto>
                {
                    Items = _mapper.Map<List<TransferDto>>(transfers),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return ApiResponse<PagedResult<TransferDto>>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged transfers");
                return ApiResponse<PagedResult<TransferDto>>.ErrorResponse("An error occurred while retrieving transfers");
            }
        }
    }
}