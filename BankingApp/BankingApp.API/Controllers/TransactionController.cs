using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.Services.Interfaces;
using BankingApp.Application.DTOs.Transaction;
using BankingApp.Application.DTOs.Common;

namespace BankingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("deposit")]
        public async Task<ActionResult<ApiResponse<TransactionDto>>> Deposit([FromBody] DepositDto dto)
        {
            var result = await _transactionService.DepositAsync(dto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("account/{accountId}")]
        public async Task<ActionResult<ApiResponse<List<TransactionDto>>>> GetTransactionsByAccountId(int accountId)
        {
            var result = await _transactionService.GetTransactionsByAccountIdAsync(accountId);
            return Ok(result);
        }

        [HttpGet("account/{accountId}/date-range")]
        public async Task<ActionResult<ApiResponse<List<TransactionDto>>>> GetTransactionsByDateRange(
            int accountId, 
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            var result = await _transactionService.GetTransactionsByDateRangeAsync(accountId, startDate, endDate);
            return Ok(result);
        }

        [HttpGet("account/{accountId}/paged")]
        public async Task<ActionResult<ApiResponse<PagedResult<TransactionDto>>>> GetTransactionsPaged(
            int accountId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _transactionService.GetTransactionsPagedAsync(accountId, pageNumber, pageSize);
            return Ok(result);
        }
    }
}