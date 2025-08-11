// BankingApp.API/Controllers/TransferController.cs
using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.DTOs.Common;
using BankingApp.Application.DTOs.Transfer;
using BankingApp.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace BankingApp.API.Controllers
{
    /// <summary>
    /// Para transferi (havale/EFT) uç noktaları.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TransferController : ControllerBase
    {
        private readonly ITransferService _transferService;
        private readonly IAccountService _accountService;
        private readonly ILogger<TransferController> _logger;

        public TransferController(
            ITransferService transferService,
            IAccountService accountService,
            ILogger<TransferController> logger)
        {
            _transferService = transferService;
            _accountService = accountService;
            _logger = logger;
        }

        /// <summary>
        /// Hesap kimlikleri ile transfer oluşturur.
        /// </summary>
        /// <param name="transferDto">Transfer isteği.</param>
        /// <returns>Oluşturulan transfer sonucu.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferDto transferDto)
        {
            try
            {
                _logger.LogInformation("Creating transfer from account {FromAccountId} to account {ToAccountId}", 
                    transferDto.FromAccountId, transferDto.ToAccountId);

                var result = await _transferService.CreateTransferAsync(transferDto);
                
                if (result.Success)
                {
                    _logger.LogInformation("Transfer created successfully: {TransferCode}", result.Data?.TransferCode);
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transfer");
                return StatusCode(500, new ApiResponse<TransferDto>
                {
                    Success = false,
                    Message = "Transfer oluşturulurken bir hata oluştu"
                });
            }
        }

        /// <summary>
        /// Hesap numaraları ile transfer oluşturur (müşteriler arası desteklenir).
        /// </summary>
        /// <param name="transferDto">Transfer isteği.</param>
        /// <returns>Transfer sonucu.</returns>
        [HttpPost("by-account-number")]
        public async Task<IActionResult> CreateTransferByAccountNumber([FromBody] CreateTransferByAccountNumberDto transferDto)
        {
            try
            {
                _logger.LogInformation("Creating transfer (by account number) from {FromAccount} to {ToAccount}",
                    transferDto.FromAccountNumber, transferDto.ToAccountNumber);

                var result = await _transferService.CreateTransferByAccountNumberAsync(
                    transferDto.FromAccountNumber,
                    transferDto.ToAccountNumber,
                    transferDto.Amount,
                    transferDto.Description);

                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transfer by account number");
                return StatusCode(500, new ApiResponse<TransferDto>
                {
                    Success = false,
                    Message = "Transfer oluşturulurken bir hata oluştu"
                });
            }
        }

        /// <summary>
        /// Bir hesaba ait transfer geçmişini getirir.
        /// </summary>
        /// <param name="accountId">Hesap kimliği.</param>
        /// <returns>Transfer listesini içeren ApiResponse.</returns>
        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetTransfersByAccount(int accountId)
        {
            try
            {
                _logger.LogInformation("Getting transfers for account {AccountId}", accountId);

                var result = await _transferService.GetTransfersByAccountAsync(accountId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transfers for account {AccountId}", accountId);
                return StatusCode(500, new ApiResponse<List<TransferDto>>
                {
                    Success = false,
                    Message = "Transfer geçmişi alınırken bir hata oluştu"
                });
            }
        }

        /// <summary>
        /// Bir müşteriye ait tüm transferleri getirir.
        /// </summary>
        /// <param name="customerId">Müşteri kimliği.</param>
        /// <returns>Transfer listesini içeren ApiResponse.</returns>
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetTransfersByCustomer(int customerId)
        {
            try
            {
                _logger.LogInformation("Getting transfers for customer {CustomerId}", customerId);

                var result = await _transferService.GetTransfersByCustomerAsync(customerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transfers for customer {CustomerId}", customerId);
                return StatusCode(500, new ApiResponse<List<TransferDto>>
                {
                    Success = false,
                    Message = "Müşteri transfer geçmişi alınırken bir hata oluştu"
                });
            }
        }

        /// <summary>
        /// Transfer kimliğine göre transfer detayını getirir.
        /// </summary>
        /// <param name="transferId">Transfer kimliği.</param>
        /// <returns>Transfer detayını içeren ApiResponse.</returns>
        [HttpGet("{transferId}")]
        public async Task<IActionResult> GetTransferById(int transferId)
        {
            try
            {
                _logger.LogInformation("Getting transfer {TransferId}", transferId);

                var result = await _transferService.GetTransferByIdAsync(transferId);
                
                if (result.Success && result.Data != null)
                {
                    return Ok(result);
                }
                
                return NotFound(new ApiResponse<TransferDto>
                {
                    Success = false,
                    Message = "Transfer bulunamadı"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transfer {TransferId}", transferId);
                return StatusCode(500, new ApiResponse<TransferDto>
                {
                    Success = false,
                    Message = "Transfer bilgisi alınırken bir hata oluştu"
                });
            }
        }

        /// <summary>
        /// Transfer kurallarını kontrol etmek için ön doğrulama yapar.
        /// </summary>
        /// <param name="transferDto">Transfer isteği.</param>
        /// <returns>Doğrulama sonucu.</returns>
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateTransfer([FromBody] CreateTransferDto transferDto)
        {
            try
            {
                _logger.LogInformation("Validating transfer from account {FromAccountId} to account {ToAccountId}", 
                    transferDto.FromAccountId, transferDto.ToAccountId);

                var result = await _transferService.ValidateTransferAsync(transferDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating transfer");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Transfer doğrulaması sırasında bir hata oluştu"
                });
            }
        }

        /// <summary>
        /// Hesap numaraları ile transfer ön doğrulaması yapar.
        /// </summary>
        /// <param name="transferDto">Transfer isteği.</param>
        /// <returns>Doğrulama sonucu.</returns>
        [HttpPost("validate/by-account-number")]
        public async Task<IActionResult> ValidateTransferByAccountNumber([FromBody] CreateTransferByAccountNumberDto transferDto)
        {
            try
            {
                _logger.LogInformation("Validating transfer (by account number) from {FromAccount} to {ToAccount}",
                    transferDto.FromAccountNumber, transferDto.ToAccountNumber);

                var result = await _transferService.ValidateTransferByAccountNumberAsync(
                    transferDto.FromAccountNumber,
                    transferDto.ToAccountNumber,
                    transferDto.Amount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating transfer by account number");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Transfer doğrulaması sırasında bir hata oluştu"
                });
            }
        }
    }
}