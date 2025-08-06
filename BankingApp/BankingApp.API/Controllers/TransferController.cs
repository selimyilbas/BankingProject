// BankingApp.API/Controllers/TransferController.cs
using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.DTOs.Common;
using BankingApp.Application.DTOs.Transfer;
using BankingApp.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace BankingApp.API.Controllers
{
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
    }
}