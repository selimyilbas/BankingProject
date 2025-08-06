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
        private readonly ILogger<TransferService> _logger;
        private readonly IMapper _mapper;

        public TransferService(IUnitOfWork unitOfWork, ILogger<TransferService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ApiResponse<TransferDto>> CreateTransferAsync(CreateTransferDto dto)
        {
            try
            {
                _logger.LogInformation("Creating transfer from account {FromAccountId} to account {ToAccountId}", 
                    dto.FromAccountId, dto.ToAccountId);

                // Validate transfer
                var validationResult = await ValidateTransferAsync(dto);
                if (!validationResult.Success)
                {
                    return ApiResponse<TransferDto>.ErrorResponse(validationResult.Message);
                }

                // Get accounts
                var fromAccount = await _unitOfWork.Accounts.GetByIdAsync(dto.FromAccountId);
                var toAccount = await _unitOfWork.Accounts.GetByIdAsync(dto.ToAccountId);

                if (fromAccount == null || toAccount == null)
                {
                    return ApiResponse<TransferDto>.ErrorResponse("Hesap bulunamadı");
                }

                // Check if accounts are active
                if (!fromAccount.IsActive || !toAccount.IsActive)
                {
                    return ApiResponse<TransferDto>.ErrorResponse("Hesap aktif değil");
                }

                // Check sufficient balance
                if (fromAccount.Balance < dto.Amount)
                {
                    return ApiResponse<TransferDto>.ErrorResponse("Yetersiz bakiye");
                }

                // Get exchange rate
                var exchangeRate = await _unitOfWork.ExchangeRates.GetCurrentRateAsync(fromAccount.Currency, toAccount.Currency);
                if (exchangeRate == null)
                {
                    return ApiResponse<TransferDto>.ErrorResponse("Döviz kuru bulunamadı");
                }

                // Calculate converted amount
                var convertedAmount = dto.Amount * exchangeRate.Rate;

                // Create transfer
                var transfer = new BankingApp.Domain.Entities.Transfer
                {
                    TransferCode = await _unitOfWork.Transfers.GenerateTransferCodeAsync(),
                    FromAccountId = dto.FromAccountId,
                    ToAccountId = dto.ToAccountId,
                    Amount = dto.Amount,
                    FromCurrency = fromAccount.Currency,
                    ToCurrency = toAccount.Currency,
                    ExchangeRate = exchangeRate.Rate,
                    ConvertedAmount = convertedAmount,
                    Status = "COMPLETED",
                    Description = dto.Description,
                    TransferDate = DateTime.UtcNow,
                    CompletedDate = DateTime.UtcNow
                };

                await _unitOfWork.Transfers.AddAsync(transfer);

                // Update account balances
                fromAccount.Balance -= dto.Amount;
                toAccount.Balance += convertedAmount;

                await _unitOfWork.SaveChangesAsync();

                var transferDto = _mapper.Map<TransferDto>(transfer);
                return ApiResponse<TransferDto>.SuccessResponse(transferDto, "Transfer başarıyla tamamlandı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transfer");
                return ApiResponse<TransferDto>.ErrorResponse("Transfer oluşturulurken bir hata oluştu");
            }
        }

        public async Task<ApiResponse<TransferDto>> GetTransferByIdAsync(int transferId)
        {
            try
            {
                var transfer = await _unitOfWork.Transfers.GetByIdAsync(transferId);
                if (transfer == null)
                {
                    return ApiResponse<TransferDto>.ErrorResponse("Transfer bulunamadı");
                }

                var transferDto = _mapper.Map<TransferDto>(transfer);
                return ApiResponse<TransferDto>.SuccessResponse(transferDto, "Transfer bilgisi başarıyla alındı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transfer {TransferId}", transferId);
                return ApiResponse<TransferDto>.ErrorResponse("Transfer bilgisi alınırken bir hata oluştu");
            }
        }

        public async Task<ApiResponse<List<TransferDto>>> GetTransfersByAccountAsync(int accountId)
        {
            try
            {
                var transfers = await _unitOfWork.Transfers.GetByAccountAsync(accountId);
                var transferDtos = _mapper.Map<List<TransferDto>>(transfers);
                return ApiResponse<List<TransferDto>>.SuccessResponse(transferDtos, "Transfer geçmişi başarıyla alındı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transfers for account {AccountId}", accountId);
                return ApiResponse<List<TransferDto>>.ErrorResponse("Transfer geçmişi alınırken bir hata oluştu");
            }
        }

        public async Task<ApiResponse<List<TransferDto>>> GetTransfersByCustomerAsync(int customerId)
        {
            try
            {
                var transfers = await _unitOfWork.Transfers.GetByCustomerAsync(customerId);
                var transferDtos = _mapper.Map<List<TransferDto>>(transfers);
                return ApiResponse<List<TransferDto>>.SuccessResponse(transferDtos, "Müşteri transfer geçmişi başarıyla alındı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transfers for customer {CustomerId}", customerId);
                return ApiResponse<List<TransferDto>>.ErrorResponse("Müşteri transfer geçmişi alınırken bir hata oluştu");
            }
        }

        public async Task<ApiResponse<object>> ValidateTransferAsync(CreateTransferDto dto)
        {
            try
            {
                // Check if accounts exist
                var fromAccount = await _unitOfWork.Accounts.GetByIdAsync(dto.FromAccountId);
                var toAccount = await _unitOfWork.Accounts.GetByIdAsync(dto.ToAccountId);

                if (fromAccount == null || toAccount == null)
                {
                    return ApiResponse<object>.ErrorResponse("Hesap bulunamadı");
                }

                // Check if accounts are active
                if (!fromAccount.IsActive || !toAccount.IsActive)
                {
                    return ApiResponse<object>.ErrorResponse("Hesap aktif değil");
                }

                // Check if same account
                if (dto.FromAccountId == dto.ToAccountId)
                {
                    return ApiResponse<object>.ErrorResponse("Aynı hesaba transfer yapılamaz");
                }

                // Check sufficient balance
                if (fromAccount.Balance < dto.Amount)
                {
                    return ApiResponse<object>.ErrorResponse("Yetersiz bakiye");
                }

                // Check amount is positive
                if (dto.Amount <= 0)
                {
                    return ApiResponse<object>.ErrorResponse("Transfer tutarı pozitif olmalıdır");
                }

                // Check exchange rate availability
                var exchangeRate = await _unitOfWork.ExchangeRates.GetCurrentRateAsync(fromAccount.Currency, toAccount.Currency);
                if (exchangeRate == null)
                {
                    return ApiResponse<object>.ErrorResponse("Döviz kuru bulunamadı");
                }

                return ApiResponse<object>.SuccessResponse(new { IsValid = true }, "Transfer doğrulaması başarılı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating transfer");
                return ApiResponse<object>.ErrorResponse("Transfer doğrulaması sırasında bir hata oluştu");
            }
        }
    }
}