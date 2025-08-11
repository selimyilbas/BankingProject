using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.Services.Interfaces;
using BankingApp.Application.DTOs.Account;
using BankingApp.Application.DTOs.Common;

namespace BankingApp.API.Controllers
{
    /// <summary>
    /// Hesap yönetimi uç noktaları: oluşturma, sorgulama, bakiye görüntüleme ve durum güncelleme.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>
        /// Yeni bir banka hesabı oluşturur.
        /// </summary>
        /// <param name="dto">Hesap oluşturma isteği.</param>
        /// <returns>Oluşturulan hesabın detaylarını içeren ApiResponse.</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<AccountDto>>> CreateAccount([FromBody] CreateAccountDto dto)
        {
            var result = await _accountService.CreateAccountAsync(dto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Hesap kimliğine göre hesap bilgilerini getirir.
        /// </summary>
        /// <param name="accountId">İç sistem hesap kimliği.</param>
        /// <returns>Hesap bilgilerini içeren ApiResponse.</returns>
        [HttpGet("{accountId}")]
        public async Task<ActionResult<ApiResponse<AccountDto>>> GetAccountById(int accountId)
        {
            var result = await _accountService.GetAccountByIdAsync(accountId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        /// <summary>
        /// Hesap numarasına göre hesap bilgilerini getirir.
        /// </summary>
        /// <param name="accountNumber">Banka hesap numarası.</param>
        /// <returns>Hesap bilgilerini içeren ApiResponse.</returns>
        [HttpGet("by-number/{accountNumber}")]
        public async Task<ActionResult<ApiResponse<AccountDto>>> GetAccountByNumber(string accountNumber)
        {
            var result = await _accountService.GetAccountByNumberAsync(accountNumber);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        /// <summary>
        /// Bir müşteriye ait tüm hesapları listeler.
        /// </summary>
        /// <param name="customerId">Müşteri kimliği.</param>
        /// <returns>Hesap listesini içeren ApiResponse.</returns>
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<ApiResponse<List<AccountDto>>>> GetAccountsByCustomerId(int customerId)
        {
            var result = await _accountService.GetAccountsByCustomerIdAsync(customerId);
            return Ok(result);
        }

        /// <summary>
        /// Verilen hesap numarasının güncel bakiyesini getirir.
        /// </summary>
        /// <param name="accountNumber">Banka hesap numarası.</param>
        /// <returns>Hesap bakiyesini içeren ApiResponse.</returns>
        [HttpGet("balance/{accountNumber}")]
        public async Task<ActionResult<ApiResponse<AccountBalanceDto>>> GetAccountBalance(string accountNumber)
        {
            var result = await _accountService.GetAccountBalanceAsync(accountNumber);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        /// <summary>
        /// Hesabın aktiflik durumunu günceller (aktif/pasif).
        /// </summary>
        /// <param name="accountId">İç sistem hesap kimliği.</param>
        /// <param name="dto">İstenen aktiflik durumu.</param>
        /// <returns>İşlem sonucunu belirten ApiResponse.</returns>
        [HttpPut("{accountId}/status")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateAccountStatus(int accountId, [FromBody] UpdateAccountStatusDto dto)
        {
            var result = await _accountService.UpdateAccountStatusAsync(accountId, dto.IsActive);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }

    /// <summary>
    /// Hesap aktiflik durumunu güncelleme isteği modeli.
    /// </summary>
    public class UpdateAccountStatusDto
    {
        /// <summary>
        /// Hesabın olması istenen aktif/pasif durumu.
        /// </summary>
        public bool IsActive { get; set; }
    }
}