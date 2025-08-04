using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.Services.Interfaces;
using BankingApp.Application.DTOs.Account;
using BankingApp.Application.DTOs.Common;

namespace BankingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<AccountDto>>> CreateAccount([FromBody] CreateAccountDto dto)
        {
            var result = await _accountService.CreateAccountAsync(dto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{accountId}")]
        public async Task<ActionResult<ApiResponse<AccountDto>>> GetAccountById(int accountId)
        {
            var result = await _accountService.GetAccountByIdAsync(accountId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        [HttpGet("by-number/{accountNumber}")]
        public async Task<ActionResult<ApiResponse<AccountDto>>> GetAccountByNumber(string accountNumber)
        {
            var result = await _accountService.GetAccountByNumberAsync(accountNumber);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<ApiResponse<List<AccountDto>>>> GetAccountsByCustomerId(int customerId)
        {
            var result = await _accountService.GetAccountsByCustomerIdAsync(customerId);
            return Ok(result);
        }

        [HttpGet("balance/{accountNumber}")]
        public async Task<ActionResult<ApiResponse<AccountBalanceDto>>> GetAccountBalance(string accountNumber)
        {
            var result = await _accountService.GetAccountBalanceAsync(accountNumber);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        [HttpPut("{accountId}/status")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateAccountStatus(int accountId, [FromBody] UpdateAccountStatusDto dto)
        {
            var result = await _accountService.UpdateAccountStatusAsync(accountId, dto.IsActive);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }

    public class UpdateAccountStatusDto
    {
        public bool IsActive { get; set; }
    }
}