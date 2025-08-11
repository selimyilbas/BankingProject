using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.Services.Interfaces;
using BankingApp.Application.DTOs.Transaction;
using BankingApp.Application.DTOs.Common;

namespace BankingApp.API.Controllers
{
    /// <summary>
    /// İşlem uç noktaları: para yatırma ve işlem geçmişi sorgulama.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        /// <summary>
        /// Hesaba para yatırma işlemi yapar.
        /// </summary>
        /// <param name="dto">Para yatırma isteği.</param>
        /// <returns>Oluşan işlem bilgilerini içeren ApiResponse.</returns>
        [HttpPost("deposit")]
        public async Task<ActionResult<ApiResponse<TransactionDto>>> Deposit([FromBody] DepositDto dto)
        {
            var result = await _transactionService.DepositAsync(dto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Hesabın tüm işlemlerini listeler.
        /// </summary>
        /// <param name="accountId">Hesap kimliği.</param>
        /// <returns>İşlem listesini içeren ApiResponse.</returns>
        [HttpGet("account/{accountId}")]
        public async Task<ActionResult<ApiResponse<List<TransactionDto>>>> GetTransactionsByAccountId(int accountId)
        {
            var result = await _transactionService.GetTransactionsByAccountIdAsync(accountId);
            return Ok(result);
        }

        /// <summary>
        /// Belirtilen tarih aralığındaki işlemleri listeler.
        /// </summary>
        /// <param name="accountId">Hesap kimliği.</param>
        /// <param name="startDate">Başlangıç tarihi (UTC).</param>
        /// <param name="endDate">Bitiş tarihi (UTC).</param>
        /// <returns>İşlem listesini içeren ApiResponse.</returns>
        [HttpGet("account/{accountId}/date-range")]
        public async Task<ActionResult<ApiResponse<List<TransactionDto>>>> GetTransactionsByDateRange(
            int accountId, 
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            var result = await _transactionService.GetTransactionsByDateRangeAsync(accountId, startDate, endDate);
            return Ok(result);
        }

        /// <summary>
        /// İşlemleri sayfalı olarak listeler.
        /// </summary>
        /// <param name="accountId">Hesap kimliği.</param>
        /// <param name="pageNumber">Sayfa numarası.</param>
        /// <param name="pageSize">Sayfa başına kayıt sayısı.</param>
        /// <returns>Sayfalı işlem sonuçlarını içeren ApiResponse.</returns>
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